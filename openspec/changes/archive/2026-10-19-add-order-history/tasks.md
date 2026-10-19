# Tasks

## 1. Consultas no Orders

- [x] 1.1 `IOrderRepository`/`OrderRepository`: `GetCustomerOrdersAsync(customerId, skip, take)` e `GetCustomerOrderAsync(customerId, orderNumber)`, filtrando por `CustomerId` na consulta, com `Include` dos itens, `AsNoTracking` e ordem `OrderDate` desc, `Id` desc; índice `(CustomerId, OrderDate)` na `OrderConfigurations`; verificar com `dotnet build Scorponok.slnx`
- [x] 1.2 `GetCustomerOrdersQuery`/`GetCustomerOrderQuery`, os DTOs (lista com `hasMore` pela linha extra, `itemCount` e foto do primeiro item; detalhe com itens, `subtotal`, endereço sem país, `attempt`, `maxAttempts` e `canRetryPayment` de `Order.CanRetryPayment`) e os handlers registrados em `AddOrderHandlers`; `pageSize` ajustado entre 1 e 50
- [x] 1.3 Testes dos handlers com o repositório mockado em `Orders.Tests/CommandHandlers`: `hasMore` com e sem a linha extra, limite de `pageSize`, contagem de itens, `canRetryPayment` em `Failed` com e sem tentativas, pedido de outro cliente → `null`, DTO sem `PaymentId`, e-mail e CPF; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 2. Endpoints do Orders

- [x] 2.1 `GET /orders` e `GET /orders/{orderNumber}` em `WebApplicationExtensions` com a política `VerifiedCustomer`, cliente do `sub`, `200`/`404 { errors: ["Pedido não encontrado."] }`; chamados no `Program.cs`
- [x] 2.2 Testes do mapeamento em `Orders.Tests/Web` (como o `RetryOrderPaymentEndpointTests`): cliente do token repassado à query, `404` para pedido não encontrado, `403` sem identidade, política exigida nos dois endpoints; verificar com `dotnet test src/backend/Store/Orders.Tests`
- [x] 2.3 Exemplos no `Orders.Web.Api.http` e seção no `CLAUDE.md` da raiz (endpoints, `hasMore`, dono, índice e recriação opcional do `orders_data`); verificar chamando os dois endpoints com um token do Keycloak no `dotnet run`

## 3. Front-end: lista

- [x] 3.1 `lib/orders.ts`: tipos e `getMyOrders(page, accessToken, signal)`/`getMyOrder(orderNumber, accessToken, signal)` (`401` → `SessionExpiredError`, `404` → `null`); `lib/orderStatus.ts` com o texto e a cor de cada situação
- [x] 3.2 `OrdersPage` em `/account/orders` (dentro do `RequireAuth`): lista com número, data pt-BR, situação, total, quantidade e foto, link para o detalhe, "Carregar mais" sem repetir `orderNumber`, estado vazio com link para a loja, `LoadingState` e `ErrorState` com tentar de novo
- [x] 3.3 "Meus pedidos" no `AccountMenu` e no menu móvel do `Header`; "Ver meus pedidos" na `OrderConfirmation`; verificar com `npm run build && npm run lint` em `src/frontend`

## 4. Front-end: detalhe

- [x] 4.1 Mover o `RetryPaymentForm` da `OrderConfirmation` para `components/orders/RetryPaymentForm.tsx`, com o botão secundário opcional; a confirmação continua igual (mesmo build e o fluxo de recusa no navegador)
- [x] 4.2 `OrderDetailPage` em `/account/orders/:orderNumber`: itens com preço unitário, desconto, quantidade e subtotal, total, endereço e situação; "pedido não encontrado" no `404`; textos de "não processado" e de tentativas esgotadas; `RetryPaymentForm` com as tentativas restantes quando `canRetryPayment`
- [x] 4.3 Detalhe `Pending` acompanha o pagamento pelo `watchOrderPayment` com a tentativa atual e recarrega o pedido no desfecho; conexão falhou → nota do e-mail; depois do `202`, acompanha a nova tentativa; verificar com `npm run build && npm run lint`
- [x] 4.4 Atualizar o `src/frontend/CLAUDE.md` (rotas, cliente, páginas, `RetryPaymentForm` compartilhado) e criar o caso de uso `usecases/Login-UC10-consultar-pedidos.md`, com a linha no `usecases/README.md`

## 5. Verificação integrada

- [x] 5.1 No compose: um cliente com pedidos aprovado (`1111`), recusado (`0000`) e pendente (`payment-api` parado) vê os 3 na ordem certa; o detalhe do pendente vira "Pago" sem recarregar ao subir o `payment-api`; o recusado paga de novo pelo detalhe; outro cliente abrindo o número vê "pedido não encontrado"; um cliente com mais de 10 pedidos usa "Carregar mais" sem repetição
  - Em 2026-10-19, `orders-api` e `frontend` reconstruídos no compose com o código novo; conferido sem token: `GET /orders` e `GET /orders/{n}` → `401`, e `/account/orders` servido pela loja. O teste no navegador, com conta real, foi feito pelo usuário em 2026-10-19 (sem client nem usuários temporários no Keycloak, a pedido). Vale também para a 2.3.
- [x] 5.2 Rodar `dotnet test Scorponok.slnx`, todos sem falhas

## Workflow follow-up

- Arquivar a change (`/opsx:archive`) depois da revisão.
