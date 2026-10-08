# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- **Orders**: `OrderPaymentHandlers.ApplyAsync` busca o pedido, aplica `ConfirmPayment`/`FailPayment` (só agem sobre `Pending`), salva e chama `IOrderPaymentNotifier` (SignalR). É chamado pelos consumidores `PaymentApproved`/`PaymentRejectedConsumer`, com `UseMessageRetry` de 3 tentativas a cada 1 s. Um resultado repetido não muda o pedido e não notifica.
- `Order` não tem e-mail. `OrderBuilder` monta o pedido a partir do `CreateCommand` (`UserId`, `Address`, `Items`, `Card`), que o endpoint recebe direto do JSON. A validação é a `OrderValidation` (FluentValidation, mensagens em inglês).
- **Front-end**: `lib/checkout.ts` (`ShippingAddress`, `PaymentDetails`, `validateCheckout`), formulário em `ShippingAddressForm`/`PaymentForm`, `OrderConfirmation` com estados `processing/approved/declined/pending` e `PAYMENT_TIMEOUT_MS = 30_000`.
- Depende de `add-order-outbox` (o `orders-db` será recriado lá também) e de `add-rabbitmq-payment-flow` (requisito "Acompanhamento do pagamento").

## Goals / Non-Goals

**Goals:**
- A atualização do status do pedido nunca espera nem falha por causa do servidor de e-mail.
- O e-mail é enviado mesmo que o servidor de e-mail fique fora por um tempo longo, e no máximo uma vez por pedido no caso normal.

**Non-Goals:**
- Mais de uma instância do `orders-api` enviando e-mails (o hub SignalR já assume uma instância).
- Fila genérica de notificações ou um contexto `Notifications` separado.

## Decisions

### 1. E-mail no pedido
- `CreateCommand` ganha `string Email`; `OrderBuilder.AddEmail(email)`; `Order.Email` (coluna `Email`, obrigatória).
- `OrderValidation`: `NotEmpty` + `EmailAddress` com mensagem "Invalid email." (a regra do FluentValidation só exige texto@texto, suficiente para o caso).
- No front: campo `email` num bloco "Contact" no topo do formulário, validado em `validateCheckout` (regex simples `^[^\s@]+@[^\s@]+\.[^\s@]+$`) e enviado no `CreateOrderRequest`.

### 2. Envio desacoplado do consumidor: marcador no pedido + serviço em segundo plano
- `Order` ganha `DateTime? PaymentEmailSentAt` e o método `MarkPaymentEmailSent()`. Pedidos com status `Confirmed` ou `Failed` e `PaymentEmailSentAt` nulo estão "devendo e-mail".
- `OrderEmailDispatcher` (`BackgroundService` no `Orders.Infrastructure`), a cada `Smtp:PollSeconds` (padrão 5 s): busca até 20 pedidos devendo e-mail (`IOrderRepository.GetAwaitingPaymentEmailAsync`), envia cada um por `IOrderEmailSender` e marca/salva pedido a pedido. Falha de SMTP: registra log e deixa o pedido para a próxima rodada.
- `OrderPaymentHandlers` não muda: o status é salvo e a loja é avisada como hoje; o e-mail sai na próxima rodada do serviço.
- `IOrderEmailSender` (em `Orders.CommandHandlers`) com `SendPaymentResultAsync(Order)`; implementação `SmtpOrderEmailSender` com **MailKit** (MIT), lendo `Smtp:Host`, `Smtp:Port`, `Smtp:From`. Texto simples, em inglês como a loja: assunto "Order A123 confirmed" / "Payment declined for order A123", corpo com número, total e próximo passo.

- *Alternativa:* enviar dentro do `OrderPaymentHandlers`, depois de salvar — se o SMTP falhar, o retry do consumidor reaplica o resultado, o pedido já não está `Pending` e o e-mail se perde; e o retry de 3 s não cobre uma queda longa.
- *Alternativa:* publicar um evento `OrderPaymentResolved` pelo outbox de consumidor e enviar num consumidor próprio — robusto, mas exige `UseEntityFrameworkOutbox` nos endpoints e redelivery atrasado (plugin do RabbitMQ) para quedas longas; mais peças para o mesmo resultado.
- *Alternativa:* `System.Net.Mail.SmtpClient` — sem pacote novo, mas a própria Microsoft o marca como não recomendado para código novo.

### 3. Confirmação no front
- `PAYMENT_TIMEOUT_MS = 10_000`.
- Textos: aprovado → "Thank you for your order!" / "Your payment was approved. We sent the confirmation to {email}."; recusado → "Payment declined" / "Your card was declined. We sent the details to {email}."; pendente → "Payment pending" / "Your order was placed and the payment is pending. We'll send the result to {email}."
- `OrderConfirmation` recebe o e-mail usado no pedido (a página já o tem no formulário).

### 4. Mailpit no compose
`docker-compose.mail.yml` com `axllent/mailpit` (SMTP 1025, painel 8025), incluído no `docker-compose.yml`; `orders-api` recebe `Smtp__Host: mailpit` e `Smtp__Port: 1025`. `appsettings.Development.json` aponta para `localhost:1025`.

### 5. Provedor SMTP real só por configuração
A seção `Smtp` aceita também `Username`, `Password` e `UseStartTls` (padrão `false`). O `SmtpOrderEmailSender` conecta com `SecureSocketOptions.StartTls` quando `UseStartTls` é `true` (senão `None`, como o Mailpit exige) e só chama `AuthenticateAsync` quando `Username` está preenchido. Assim, enviar de verdade a partir da máquina local (ex.: `smtp.gmail.com:587` com senha de app, ou SendGrid/Brevo/Mailgun/SES) é só trocar a configuração.

- A senha nunca vai para o repositório: no `dotnet run` vem do `dotnet user-secrets` (o `Orders.Web.Api` ganha `UserSecretsId`); no compose, da variável `Smtp__Password` lida de um `.env` fora do git (o `.gitignore` passa a ignorar `.env`).
- `From` precisa ser um remetente aceito pelo provedor (o próprio e-mail no Gmail, ou um remetente verificado nos serviços).

- *Alternativa:* SSL implícito na porta 465 (`SslOnConnect`) — também suportado pelo MailKit, mas a 587 com STARTTLS é aceita por todos os provedores citados; uma opção a mais não se paga agora.

## Risks / Trade-offs

- [Atraso de até `PollSeconds` entre o resultado e o e-mail] → aceitável para e-mail; configurável.
- [Envio e marcação não são atômicos: se o processo cair depois de enviar e antes de salvar, o cliente recebe dois e-mails] → janela pequena; aceito (entrega "ao menos uma vez").
- [Consulta periódica ao `orders-db`] → filtro por status e `PaymentEmailSentAt` nulo, limitado a 20 por rodada; volume baixo neste projeto.
- [Pedidos antigos sem e-mail] → o volume é recriado; não há pedidos antigos.
- [Mailpit não é um servidor real] → um provedor SMTP real pode ser usado só trocando a configuração `Smtp` (decisão 5); entregabilidade de produção (domínio próprio, SPF/DKIM) continua fora do escopo.
- [Senha do SMTP vazar pelo git] → user-secrets e `.env` ignorado; nenhum `appsettings*.json` versionado recebe `Password`.
- [Limites dos provedores gratuitos (ex.: cota diária do Gmail)] → uso só para testes manuais; os testes automatizados e ponta a ponta usam o Mailpit.

## Migration Plan

1. Recriar o volume do `orders-db` (coluna `Email` obrigatória e `PaymentEmailSentAt`).
2. Subir o `mailpit` junto com o resto (`docker compose up -d --build`).
3. Rollback: voltar o código; front e API antigos não enviam nem exigem `email`.
