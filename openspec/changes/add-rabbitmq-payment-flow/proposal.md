# Proposal

## Why

O pedido nasce `Pending` e nunca sai disso: o cartão chega ao Orders e é ignorado, e o contexto Payments só guarda tipos sem uso. O `use-case.md` diz que o pedido só é confirmado depois do pagamento aprovado. Esta change liga os dois contextos de forma assíncrona, por mensagens no RabbitMQ, sem acoplar o Orders à disponibilidade do Payments.

## What Changes

- **Infra**: container RabbitMQ (com painel de gerenciamento) no compose; Orders e Payments se conectam a ele.
- **Orders**:
  - Depois de registrar o pedido, publica a solicitação de pagamento com o número do pedido, o total calculado pelo próprio pedido e os dados do cartão.
  - Consome o resultado: aprovado → `Confirmed` e guarda o `PaymentId`; recusado → `Failed`.
  - Notifica a loja em tempo real (SignalR) quando o resultado do pagamento atualiza o pedido.
  - O `POST /createOrder` não muda de contrato: continua respondendo `201` com o pedido `Pending`.
- **Payments**:
  - O `Gateway.Payment.Web.Api` deixa de ser o template e passa a consumir as solicitações.
  - Processa no gateway (stub: cartão terminado em `0000` é recusado), registra o pagamento no `payment-db` e publica aprovado ou recusado.
  - Não guarda CVV nem o número completo do cartão.
  - Uma solicitação repetida não gera um segundo pagamento.
- **Checkout**: depois do `201`, a confirmação mostra "pagamento em processamento" e recebe do Orders a mensagem com o resultado (aprovado ou recusado), sem consultar o pedido periodicamente.

## Capabilities

### New Capabilities
- `payment-processing`: como o Payments recebe uma solicitação de pagamento, decide, registra e comunica o resultado.

### Modified Capabilities
- `order-placement`: o pedido registrado passa a solicitar o pagamento, reage ao resultado e avisa a loja.
- `checkout`: a confirmação recebe o resultado do pagamento em vez de mostrar só o pedido pendente.

## Impact

- **Pacotes**: MassTransit e MassTransit.RabbitMQ 8.5.10 (Apache-2.0) no `Directory.Build.props`; Npgsql no Payments; `@microsoft/signalr` no front-end.
- **Shared**: contratos das mensagens de integração.
- **Orders**: handler de criação, novos consumidores, repositório (busca por número), hub SignalR e CORS com credenciais.
- **Payments**: domínio do pagamento refeito (sem CVV), DbContext no `payment-db`, consumidor, API.
- **Front-end**: conexão com o hub e tela de confirmação.
- **Compose e docs**: serviço `rabbitmq`, variáveis de conexão, `CLAUDE.md`.
- O banco do Payments ganha tabela nova; o do Orders não muda de esquema.

## Fora do escopo

- Gateway real e regras de antifraude: o gateway continua um stub.
- Outbox transacional: se a publicação falhar depois de salvar, o pedido fica `Pending` (risco registrado no design).
- Tentar pagar de novo um pedido `Failed` ou trocar de cartão.
- Consulta do pedido por HTTP (`GET /orders/...`) e histórico de pedidos.
- Mais de uma instância do Orders (exigiria backplane do SignalR).
- Event sourcing do pagamento (`Gateway.Payment.Data` mantém o event store como está).
- Tokenização do cartão e criptografia das mensagens.
