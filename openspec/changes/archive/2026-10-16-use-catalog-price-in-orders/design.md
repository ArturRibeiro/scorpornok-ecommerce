# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- `OrderHandler` monta o `Order` com o `OrderBuilder` a partir do `CreateCommand`; o `CreateOrderItem` copia `UnitPrice`, `ProductName` e `PictureUrl` do `OrderItemMessageResponse`, que vem do corpo do `POST /createOrder`. O `Order.Total` (soma de `UnitPrice × Quantity`) é o valor do `PaymentRequested`.
- O `OrderItemValidation` exige nome, imagem e preço maior que zero, e a validação do `Order` vira `CreateOrderResult.Invalid` → `400 { errors }`.
- O Catalog expõe só `GET /Products` (paginado, até 20) e `GET /GetProductById/{id}`. `Product.Id` é `long`; `OrderItem.ProductId` é `int`.
- O Orders não conhece o Catalog hoje: nenhuma configuração, cliente HTTP ou dependência no compose.
- O `OrderHandler` grava o pedido e a mensagem do outbox dentro de `ExecuteInTransactionAsync`.

## Goals / Non-Goals

**Goals:**
- Os handlers continuam sem depender de HTTP: o `OrderHandler` usa uma porta, implementada na infraestrutura.
- Uma única chamada ao Catalog por pedido, fora da transação do banco.
- O front-end funciona sem alteração.

**Non-Goals:**
- Retentativas, circuit breaker ou cache dos preços.
- Remover `unitPrice`, `productName`, `pictureUrl` e `discount` do contrato do `POST /createOrder` (passam a ser ignorados).

## Decisions

### 1. Endpoint em lote no Catalog
`GET /GetProductsByIds?ids=1&ids=2` (minimal API, `long[] ids` da query), que devolve a lista de `ProductItemMessageResponse` dos produtos encontrados; os inexistentes simplesmente não aparecem. Sem `ids` ou com mais de 100, `400`. Nova consulta `IProductQueries.GetProductsByIds`.

- *Alternativa:* uma chamada a `GetProductById` por item: N requisições por pedido e mais pontos de falha.
- *Alternativa:* `POST` com o corpo de ids: funciona, mas é uma leitura; o `GET` segue o estilo dos endpoints atuais.

### 2. Porta `ICatalogProducts` no `Orders.CommandHandlers`
```csharp
public interface ICatalogProducts
{
    // Só os produtos encontrados; lança CatalogUnavailableException se o Catalog não responder.
    Task<IReadOnlyDictionary<int, CatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds, CancellationToken ct);
}
public record CatalogProduct(int Id, string Name, string PictureUrl, decimal Price);
```
Implementação `HttpCatalogProducts` no `Orders.Infrastructure`, como `HttpClient` tipado (`AddHttpClient`), com `BaseAddress` de `Catalog:BaseAddress` e `Timeout` de 5 s. Status diferente de 2xx, `HttpRequestException` ou tempo esgotado viram `CatalogUnavailableException`.

- *Alternativa:* chamar o `HttpClient` direto no handler: acopla o handler ao transporte e complica os testes.

### 3. Fluxo do `OrderHandler`
1. Ids distintos dos itens; se houver itens, uma chamada ao `ICatalogProducts`, **antes** de abrir a transação.
2. `CatalogUnavailableException` → `CreateOrderResult.Unavailable()`, sem tocar no banco.
3. Cada item é criado com `Name`, `PictureUrl` e `Price` do Catalog e a quantidade do pedido. Produto ausente → erro `Produto {id} não encontrado.`, somado aos erros do `order.IsValid()`; havendo qualquer erro, `Invalid`.
4. O restante (transação, outbox, `PaymentRequested` com `order.Total`) não muda.

### 4. Resultado com três desfechos
`CreateOrderResult` ganha um `Outcome` (`Created`, `Invalid`, `Unavailable`) no lugar do `bool Success`. O endpoint mapeia `Unavailable` → `Results.Problem(statusCode: 503, title: "Catalog unavailable")`. O front-end já trata status fora de 201/400/401 como falha de envio, mantendo o carrinho.

### 5. Configuração
- `appsettings.Development.json` do Orders: `Catalog:BaseAddress` = `http://localhost:5064`.
- `docker-compose.yml`: `Catalog__BaseAddress: http://catalog-api:8080` no `orders-api` e `depends_on` do `catalog-api` (`service_started`).
- Sem a configuração, a API não sobe (mesmo padrão da connection string).

## Risks / Trade-offs

- [O registro passa a depender do Catalog] → `503` claro e nada persistido; o carrinho é mantido e o cliente pode tentar de novo. A réplica de preços fica como evolução (fora do escopo).
- [O total cobrado pode diferir do exibido no carrinho] → a confirmação mostra o total devolvido pelo Orders, e o e-mail traz o total cobrado.
- [`long` no Catalog e `int` no Orders] → os ids do seed cabem em `int`; o cliente converte e descarta ids fora da faixa (tratados como inexistentes).
- [Latência extra no `POST`] → uma chamada em lote, com tempo-limite de 5 s.

## Migration Plan

- Sem mudança de banco nem de mensagens. Basta subir o Catalog antes do Orders; o compose já garante a ordem.
- Rollback: reverter o commit; o contrato do `POST /createOrder` não muda.
