# Tasks

## 1. Outbox no Orders.Infrastructure

- [x] 1.1 Adicionar `MassTransit.EntityFrameworkCore` (`$(MassTransit)`) ao `Orders.Infrastructure.csproj`; verificar com `dotnet build Scorponok.sln`
- [x] 1.2 Adicionar ao `OrderContext` as entidades do outbox (`AddInboxStateEntity`, `AddOutboxMessageEntity`, `AddOutboxStateEntity`) e configurar `AddEntityFrameworkOutbox<OrderContext>` com `UsePostgres()` e `UseBusOutbox()` no `AddInfrastructure`; verificar recriando o volume do `orders-db`, subindo com `docker compose up -d --build orders-api` e conferindo no `psql` as tabelas do outbox no schema `order` e o `/health/ready` 200
- [x] 1.3 Criar `IOrderRepository.ExecuteInTransactionAsync(Func<Task>, CancellationToken)` e implementá-lo no `OrderRepository` com `BeginTransactionAsync` (commit no fim, rollback em exceção); verificar com `dotnet build Scorponok.sln`

## 2. OrderHandler

- [x] 2.1 Reescrever o `OrderHandler` para gravar dentro de `ExecuteInTransactionAsync`: `Save` → `SaveEntitiesAsync` → `PublishAsync(PaymentRequested)` → `SaveEntitiesAsync`, sem o `try/catch` da publicação (pedido sem cartão continua só com log); comentar no código por que a publicação vem antes do último `SaveEntitiesAsync`
- [x] 2.2 Atualizar `OrderHandlerTests`: ordem das chamadas dentro da transação (`MockSequence`), total do pedido no `PaymentRequested`, exceção do publicador propagada (substitui o teste antigo de "devolve sucesso"), pedido sem cartão sem publicação, pedido inválido sem transação; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 3. Verificação ponta a ponta (compose)

- [x] 3.1 Fluxo normal: `POST /createOrder` com cartão final `1111` termina `Confirmed` e final `0000` termina `Failed`; a tabela de mensagens do outbox fica vazia depois da entrega
- [x] 3.2 Broker parado: `docker compose stop rabbitmq`, `POST /createOrder` responde `201` em até 5 s (medir com `curl -w %{time_total}`), o pedido fica `Pending` com uma mensagem no outbox; `docker compose start rabbitmq` e o pedido termina `Confirmed`
- [x] 3.3 Reinício: com o broker parado, criar um pedido, `docker compose restart orders-api`, subir o broker e verificar que o pedido termina `Confirmed`
- [x] 3.4 Checkout no navegador (Playwright, script no scratchpad) com o broker parado: a confirmação aparece com o número do pedido e mostra "Still processing"; com o broker no ar, mostra "Approved"

## 4. Documentação e integração

- [x] 4.1 Atualizar o `CLAUDE.md` da raiz: outbox no fluxo do pagamento (o `POST` não depende do broker; mensagem gravada com o pedido e entregue em segundo plano), tirar "se a publicação falhar depois de salvar, o pedido fica `Pending` (não há outbox)", citar `docs/diagrams/order-outbox.excalidraw` e registrar que o cartão fica no `orders-db` até a entrega; verificar que o texto bate com o código
- [x] 4.2 Rodar `dotnet test Scorponok.sln` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas

## Workflow follow-up

- Arquivar `add-checkout-page` e `add-rabbitmq-payment-flow` antes desta, porque esta adiciona requisitos ao `order-placement` que dependem deles.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
