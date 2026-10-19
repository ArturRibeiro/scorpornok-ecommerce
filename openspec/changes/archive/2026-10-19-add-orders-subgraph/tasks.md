# Tasks

## 1. Orders

- [x] 1.1 `GraphQL/` no `Orders.Web.Api`: `myOrders`, `myOrder`, `placeOrder` e `retryOrderPayment` com `[Authorize]` e `UserError`; verificar pelo `Orders.Web.Api.http`
- [x] 1.2 `schema export`, `orders` no script e `.far` recompostos; timeout do `fusion` em 15 s
- [x] 1.3 `Orders.Tests`: cada desfecho no seu código, cliente do token, pedido de outro cliente; `SchemaTests` com o Orders; verificar com `dotnet test`

## 2. Loja

- [x] 2.1 `lib/orders.ts` pelo gateway (o hub continua); verificar com `npm run build && npm run lint`
- [x] 2.2 No navegador: compra aprovada e recusada, nova tentativa, Catalog fora no checkout, "Meus pedidos" e pedido de outro cliente
  - Testado no navegador pelo usuário em 2026-10-19 (cenário 31 do roteiro), com conta real.

## 3. Docs e testes

- [x] 3.1 `CLAUDE.md` (raiz e front-end), `docs/` e `.http`
- [x] 3.2 `dotnet test Scorponok.slnx` sem falhas

## Workflow follow-up

- Aplicar só depois de arquivar a `add-customers-subgraph`.
- Próxima: subscription no lugar do SignalR, com cliente GraphQL na loja.
