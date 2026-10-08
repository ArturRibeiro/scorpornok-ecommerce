# Tasks

## 1. E-mail no pedido (Orders)

- [x] 1.1 Adicionar `Order.Email`, `OrderBuilder.AddEmail`, `CreateCommand.Email`, a regra `NotEmpty` + `EmailAddress` na `OrderValidation` e a coluna `Email` na `OrderConfigurations`; verificar com testes: pedido com e-mail válido é salvo com o e-mail, sem e-mail ou com "cliente@" é inválido com a mensagem "Invalid email."
- [x] 1.2 Atualizar o `.http` do `Orders.Web.Api` (e-mail no pedido válido, caso inválido sem e-mail); verificar recriando o volume do `orders-db`, subindo o `orders-api` e enviando os dois pedidos: `201` e `400` com "Invalid email."

## 2. Envio do e-mail (Orders)

- [x] 2.1 Adicionar `Order.PaymentEmailSentAt` com `MarkPaymentEmailSent()` e `IOrderRepository.GetAwaitingPaymentEmailAsync(limit)` (status `Confirmed`/`Failed` e marcador nulo); verificar com testes de domínio do marcador e com `dotnet build Scorponok.sln`
- [x] 2.2 Criar `IOrderEmailSender` e `SmtpOrderEmailSender` (MailKit em `$(MailKit)` no `Directory.Build.props`, seção `Smtp` com `Host`, `Port`, `From`, `Username`, `Password`, `UseStartTls`), com os textos de confirmado e recusado; STARTTLS só com `UseStartTls`, autenticação só com `Username` preenchido; verificar com testes unitários do texto montado para os dois resultados e da escolha de conexão (sem TLS/sem autenticação para o Mailpit; STARTTLS + autenticação para um provedor)
- [x] 2.3 Criar o `OrderEmailDispatcher` (`BackgroundService`, `Smtp:PollSeconds`, até 20 por rodada, marca e salva pedido a pedido, falha só registra log) e registrá-lo no `AddInfrastructure`; verificar com testes do dispatcher com repositório e sender mockados: envia e marca, falha não marca, pedido já marcado não é buscado

## 3. Mailpit e verificação ponta a ponta

- [x] 3.1 Criar `docker-compose.mail.yml` com `axllent/mailpit` (1025, 8025), incluí-lo no `docker-compose.yml`, passar `Smtp__Host`/`Smtp__Port` ao `orders-api` e `Smtp` no `appsettings.Development.json`; verificar com `docker compose up -d --build` e o painel em `http://localhost:8025`
- [x] 3.2 Pedidos com cartão final `1111` e `0000`: cada um gera exatamente um e-mail no Mailpit (consultar `http://localhost:8025/api/v1/messages`) com número, total e resultado corretos
- [x] 3.3 Mailpit parado: o pedido fica `Confirmed` na hora e o e-mail chega depois que o Mailpit volta; reenviar o `PaymentApproved` pelo painel do RabbitMQ não gera segundo e-mail
- [x] 3.4 Preparar o envio por provedor real sem expor a senha: `UserSecretsId` no `Orders.Web.Api`, `.env` no `.gitignore`, `Smtp__Username`/`Smtp__Password`/`Smtp__UseStartTls` repassados do `.env` ao `orders-api` no compose (vazios por padrão); verificar que `git status` não mostra o `.env`, que o fluxo com o Mailpit continua funcionando sem essas variáveis e que `dotnet user-secrets list --project src/backend/Store/Orders.Web.Api` lê o segredo configurado

## 4. Front-end

- [x] 4.1 Adicionar `email` ao formulário (bloco "Contact"), a `validateCheckout` (vazio e formato) e ao `CreateOrderRequest`; verificar com `npm run build && npm run lint` e no navegador: e-mail vazio e "cliente@" bloqueiam o envio com a mensagem no campo
- [x] 4.2 Mudar a `OrderConfirmation` (prazo de 10 s, textos com o e-mail para aprovado, recusado e pendente); verificar no navegador (Playwright, script no scratchpad): aprovado (`1111`), recusado (`0000`) e pendente com o RabbitMQ parado, este último em até ~10 s, e o e-mail correspondente no Mailpit depois que o broker volta

## 5. Documentação e integração

- [x] 5.1 Atualizar o `CLAUDE.md` da raiz (e-mail no `POST /createOrder`, `OrderEmailDispatcher`, seção `Smtp`, Mailpit e comando para subi-lo) e o `src/frontend/CLAUDE.md` (campo de e-mail, prazo e estados da confirmação); incluir no `CLAUDE.md` da raiz como enviar de verdade por um provedor (exemplo do Gmail com senha de app, via user-secrets ou `.env`); verificar que o texto bate com o código
- [x] 5.2 Rodar `dotnet test Scorponok.sln` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas

## Workflow follow-up

- Opcional, manual e fora do progresso: com as suas credenciais, testar o envio real (ex.: Gmail) seguindo o `CLAUDE.md`.
- Concluir e arquivar `add-checkout-page`, `add-rabbitmq-payment-flow` e `add-order-outbox` antes desta: ela modifica o requisito "Acompanhamento do pagamento", que só existe nas specs principais depois do arquivamento de `add-rabbitmq-payment-flow`.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
