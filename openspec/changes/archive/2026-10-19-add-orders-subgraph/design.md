# Design

## Context

Motivação em `proposal.md`; requisitos em `specs/`.

- Orders: `POST /createOrder` (`CreateOrderResult`: `Created`, `Invalid`, `Unavailable`), `POST /orders/{n}/payment` (`RetryOrderPaymentResult`: `Accepted`, `Invalid`, `NotFound`, `Conflict`), `GET /orders` e `GET /orders/{n}` (`CustomerOrdersHandler`), todos com `VerifiedCustomer`; hub `/hubs/orders`.
- `add-customers-subgraph`: gateway valida e repassa o token; `[Authorize]` no subgraph; payload com lista de erros.
- Loja: `createOrder`, `retryOrderPayment`, `getMyOrders`, `getMyOrder` em `lib/orders.ts`; `watchOrderPayment` em `lib/orderHub.ts`.

## Goals / Non-Goals

**Goals:** reaproveitar os handlers sem mudar regra; erros com código estável para a loja decidir a tela.

**Non-Goals:** subscription; federação do produto dentro do pedido.

## Decisions

1. **Schema**: `myOrders(page, pageSize)` e `myOrder(orderNumber)` (nulo se não encontrado), sobre os DTOs do `CustomerOrdersHandler`; `placeOrder(input)` e `retryOrderPayment(input)`, com payload `{ order | orderNumber, status, attempt..., errors: [UserError!]! }`.
2. **Erros**: `UserError { code, message }`, com `code` em `VALIDATION`, `CATALOG_UNAVAILABLE`, `NOT_FOUND`, `CONFLICT`, um por desfecho de hoje; as mensagens vêm dos handlers. *Alternativa:* o `FieldError` do Customers; descartado porque os erros do pedido são mensagens, não chaves de campo.
3. **Acesso**: `[Authorize(Policy = VerifiedCustomer)]` em todos os campos, cliente e e-mail dos claims, como nos endpoints.
4. **Hub e CORS**: o hub e o CORS do Orders continuam (WebSocket do SignalR); o `/graphql` só o gateway chama.
5. **Loja**: `lib/orders.ts` mantém as assinaturas sobre o `graphqlRequest`; `VITE_ORDERS_API_URL` fica só para o hub.
6. **Composição**: `orders` no script (`localhost:5224` e `orders-api:8080`) e no `SchemaTests`.

## Risks / Trade-offs

- [Timeout de 5 s do gateway e o `placeOrder`, que consulta o Catalog com limite de 5 s] → timeout do cliente `fusion` sobe para 15 s.
- [Mutation repetida por retentativa do cliente] → a loja não repete o `placeOrder`; o comportamento é o do `POST` de hoje.

## Migration Plan

Subir `orders-api`, `gateway-api` e o `frontend` reconstruído. Rollback: reverter e reconstruir o `frontend` (o REST continua).
