# Proposal

## Why

Hoje o resultado do pagamento só chega ao cliente se ele continuar na tela de confirmação. Se ele sair, ou se o resultado demorar mais de 30 s (RabbitMQ fora, Payments parado), ele nunca fica sabendo se a compra foi concluída, e um pagamento recusado passa despercebido. O checkout não pede e-mail, então não há outro canal para avisá-lo.

## What Changes

- **Checkout**:
  - Novo campo obrigatório de e-mail, enviado junto com o pedido.
  - A confirmação continua esperando o resultado pelo hub, com prazo menor (10 s): aprovado → sucesso; recusado → pagamento recusado; sem resultado no prazo → "pagamento pendente, você receberá o resultado por e-mail".
- **Orders**:
  - `POST /createOrder` passa a exigir `email` (formato válido); o pedido guarda o e-mail. **BREAKING** para quem chama a API sem `email` (hoje só o front-end).
  - Quando o resultado do pagamento atualiza o pedido, o Orders envia ao cliente um e-mail com o número do pedido, o total e o resultado (confirmado ou recusado). Uma falha no envio é tentada de novo sem desfazer a atualização do pedido, e o mesmo resultado não gera um segundo e-mail.
- **Infra**: Mailpit no compose, que recebe os e-mails por SMTP e os mostra numa página web, sem enviar nada para fora.

## Capabilities

### New Capabilities
- `order-notification`: aviso por e-mail ao cliente sobre o resultado do pagamento do pedido.

### Modified Capabilities
- `order-placement`: o pedido passa a exigir e guardar o e-mail do cliente.
- `checkout`: coleta do e-mail, envio dele no pedido e novos estados/prazo da confirmação.

## Impact

- **Front-end**: `lib/checkout.ts` (validação), formulário do checkout, `lib/orders.ts` (payload), `OrderConfirmation` (textos e prazo).
- **Orders**: `CreateCommand`, `Order`/`OrderBuilder`/validação, `OrderPaymentHandlers`, envio de e-mail (pacote MailKit, MIT) na Infrastructure, configuração `Smtp`.
- **Compose e docs**: serviço `mailpit` (SMTP 1025, painel 8025), variáveis do `orders-api`, `CLAUDE.md` da raiz e do front-end.
- **Dados**: colunas novas no `Order`; sem migrations, o volume do `orders-db` precisa ser recriado.
- **Dependência**: modifica requisitos criados por `add-rabbitmq-payment-flow` (acompanhamento do pagamento) e deve ser arquivada depois dela e de `add-order-outbox`.

## Fora do escopo

- E-mail de "pedido recebido" no momento do registro.
- Login, conta do cliente e página "Meus pedidos".
- Envio real de e-mail (provedor, domínio, SPF/DKIM) e templates HTML elaborados: o e-mail é texto simples.
- Tentar pagar de novo um pedido recusado.
- Distinguir na tela o motivo do atraso (broker fora ou Payments lento).
- Confirmação ou verificação do endereço de e-mail.
