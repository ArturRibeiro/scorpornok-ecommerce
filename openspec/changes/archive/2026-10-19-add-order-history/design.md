# Design

## Context

A motivação está em `proposal.md` e os requisitos em `specs/order-history/spec.md`. O estado atual que molda a abordagem:

- O `Order` já guarda tudo o que o histórico mostra: `CustomerId` (o `sub` do token), `OrderNumber`, `OrderDate`, `Status`, `Total`, `Address` e os `OrderItem` com `ProductName`, `PictureUrl`, `UnitPrice`, `Discount` e `Quantity`, copiados do Catalog no registro. O `OrderPayment` guarda `Attempt`, e `Order.CanRetryPayment`/`MaxPaymentAttempts` já definem a nova tentativa.
- O Orders só tem endpoints de escrita (`POST /createOrder` e `POST /orders/{orderNumber}/payment`). O `201` já aponta para `/orders/{orderNumber}` no `Location`, que hoje não existe. O `GetByNumberAsync` não carrega os itens.
- O dono é conferido em três lugares (`RetryOrderPaymentHandler`, `GetOrderStatusQuery` no hub), e "outro cliente" e "inexistente" respondem igual.
- O `Total` é a soma de `UnitPrice × Quantity`. O `Discount` é gravado, mas não entra no total.
- O front-end já tem `RequireAuth`, `useRequest`, `ErrorState`/`LoadingState`, `watchOrderPayment` (hub, por tentativa) e o `RetryPaymentForm`, que hoje é privado da `OrderConfirmation`.
- O Orders liga `Npgsql.EnableLegacyTimestampBehavior`, então `OrderDate` volta com o fuso do servidor e é serializado com offset.

## Goals / Non-Goals

**Goals:**
- Consultas só de leitura, sem passar pelo agregado nem pelo rastreamento do EF, e sem chamar o Catalog ou o Customers.
- A mesma regra de dono e de "não encontrado" dos endpoints que já existem.
- Reaproveitar no detalhe o hub e o formulário de nova tentativa, sem duplicar a lógica.

**Non-Goals:**
- Mudar o registro, o pagamento, as mensagens ou o modelo do EF (o índice novo é a única mudança no banco).
- Paginação por cursor ou contagem exata de páginas na tela.

## Decisions

### 1. Endpoints `GET /orders` e `GET /orders/{orderNumber}`
Os dois exigem a política `VerifiedCustomer` (sem token → `401`, e-mail não confirmado → `403`), e o cliente vem do `sub`. A coleção `/orders` já é implicitamente "do cliente do token", e `/orders/{orderNumber}` é o recurso que o `Location` do `201` já anuncia.
- `GET /orders?page=1&pageSize=10` → `200 { items, page, pageSize, hasMore }`. `pageSize` vai de 1 a 50 (padrão 10), e valores fora disso são ajustados ao limite, sem `400`. Cada item traz `orderNumber`, `orderDate`, `status`, `total`, `itemCount` (a soma das quantidades) e `pictureUrl` (do primeiro item).
- `GET /orders/{orderNumber}` → `200` com `orderNumber`, `orderDate`, `status`, `total`, `attempt`, `maxAttempts`, `canRetryPayment`, `address` (sem `country`) e `items[]` (`productId`, `productName`, `pictureUrl`, `unitPrice`, `discount`, `quantity`, `subtotal`). Para outro cliente ou número inexistente, `404 { errors: ["Pedido não encontrado."] }`, a mesma mensagem do retry.
- A resposta é um DTO montado explicitamente: `PaymentId`, `Email`, `CustomerId` e os campos de e-mail e consulta do `OrderPayment` nunca entram nele.
- *Alternativa:* `/orders/me` como no Customers. Foi descartada porque deixaria dois caminhos para o mesmo pedido (`/orders/me/{n}` e o `Location` `/orders/{n}`).

### 2. `hasMore` em vez do total
A lista pede `pageSize + 1` linhas e responde `hasMore` se vier a extra, sem um `COUNT`. A tela só precisa saber se mostra "Carregar mais".
- *Alternativa:* `IPagedList` do `Frameworker.EntityFrameworkCore`. Daria o total, mas ao custo de um `COUNT` por página e de uma dependência nova no Orders, sem uso na tela.

### 3. Queries no `Orders.CommandHandlers`, projeção no repositório
`GetCustomerOrdersQuery(CustomerId, Page, PageSize)` e `GetCustomerOrderQuery(CustomerId, OrderNumber)` passam pelo `IMemoryBus.RequestAsync`, como o `GetOrderStatusQuery`. O `IOrderRepository` ganha `GetCustomerOrdersAsync` e `GetCustomerOrderAsync`, que filtram por `CustomerId` **na consulta** (outro cliente = nenhuma linha), ordenam por `OrderDate` desc e depois `Id` desc (desempate estável) e usam `AsNoTracking` com `Include` dos itens. O handler monta o DTO e calcula `canRetryPayment` com `Order.CanRetryPayment`, para a regra continuar no agregado.
- *Alternativa:* projeção direta em SQL (`Select` para o DTO). Seria mais enxuta no banco, mas duplicaria a regra de nova tentativa e o mapeamento do `Status` (owned). Com 10 pedidos por página, o custo não pesa.

### 4. Índice `(CustomerId, OrderDate)`
O índice entra na `OrderConfigurations`. Como não há migrations, ele só aparece em bancos novos. Num volume existente as consultas funcionam sem ele, então recriar o `orders_data` é recomendado, mas não obrigatório.

### 5. Situação no front-end
A API devolve `status` com o nome do enum (`Pending`, `Confirmed`, `Failed`, `NotProcessed`), e o front-end traduz em `lib/orderStatus.ts` ("Aguardando pagamento", "Pago", "Pagamento recusado", "Pagamento não processado"), com a cor de cada uma. Um nome desconhecido aparece como veio. O `StatusDescription` do banco ("Falha no pagamento") não é usado, porque o texto da loja é outro.

### 6. Páginas e reaproveitamento
- `/account/orders` (`OrdersPage`) e `/account/orders/:orderNumber` (`OrderDetailPage`) ficam dentro do `RequireAuth`, com o cliente `lib/orders.ts` (`getMyOrders`, `getMyOrder`: `401` vira `SessionExpiredError`, e o `404` do detalhe vira `null`).
- "Carregar mais" acrescenta a próxima página e descarta os `orderNumber` já exibidos. Isso cobre um pedido novo registrado em outra aba, que empurra a lista e repetiria um item.
- O `RetryPaymentForm` sai da `OrderConfirmation` para `components/orders/RetryPaymentForm.tsx`, e as duas telas o usam. O botão secundário vira uma prop: "Voltar ao carrinho" na confirmação e nenhum no detalhe.
- No detalhe `Pending`, a página chama `watchOrderPayment(orderNumber, getAccessToken, signal, attempt)` sem prazo e, no desfecho, recarrega o pedido (`retry` do `useRequest`), para que a situação, as tentativas e `canRetryPayment` venham da API. Se a conexão falhar, a página continua em "Aguardando pagamento", com uma nota de que o resultado chega por e-mail. Depois de uma nova tentativa aceita (`202`), a página passa a acompanhar a tentativa devolvida.
- "Meus pedidos" entra no `AccountMenu` e no menu móvel do `Header`, e "Ver meus pedidos" entra na `OrderConfirmation`, ao lado de "Continuar comprando".

## Risks / Trade-offs

- [Paginação por offset com pedido novo no meio] → o front descarta os repetidos; nenhum pedido se perde, porque a inserção só empurra a lista para baixo.
- [Detalhe aberto mantém uma conexão do hub] → ela só abre em pedido `Pending` e fecha ao sair da página ou com o desfecho.
- [Volume antigo sem o índice] → as consultas ficam corretas, mas mais lentas com muitos pedidos. Está documentado no `CLAUDE.md`.
- [Desconto exibido e total sem desconto] → o subtotal é `preço × quantidade`, igual ao total cobrado, e o desconto aparece como foi gravado, só quando é maior que zero (hoje é sempre 0, porque o preço vem do Catalog).

## Migration Plan

- Não há mudança de contrato nem de mensagens: é só fazer o deploy do `orders-api` e do `frontend`.
- O índice exige recriar o volume `orders_data` (opcional).
- Rollback: reverter o commit. Os endpoints novos são só de leitura.
