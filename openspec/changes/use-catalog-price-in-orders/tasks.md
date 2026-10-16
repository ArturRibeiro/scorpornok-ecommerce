# Tasks

## 1. Endpoint em lote no Catalog

- [ ] 1.1 Adicionar `IProductQueries.GetProductsByIds(IReadOnlyCollection<long>)` e implementá-lo na consulta existente (uma query `WHERE Id IN`, projetando para `ProductItemMessageResponse`); verificar com `dotnet build Scorponok.slnx`
- [ ] 1.2 Mapear `GET /GetProductsByIds?ids=..` no `WebApplicationExtensions` do Catalog e chamá-lo no `Program.cs`: `400` sem `ids` ou com mais de 100; lista só com os encontrados; acrescentar a requisição ao `Catalog.Web.Api.http`
- [ ] 1.3 Cenário no `Catalog.feature` (SpecFlow): buscar dois ids existentes e um inexistente devolve só os dois; verificar com `dotnet test src/backend/Tests/Ecommerce.Scenarios.Integration.Spec.Tests`

## 2. Porta e cliente HTTP no Orders

- [ ] 2.1 Criar no `Orders.CommandHandlers` a porta `ICatalogProducts`, o `CatalogProduct` e a `CatalogUnavailableException`
- [ ] 2.2 Implementar `HttpCatalogProducts` no `Orders.Infrastructure` (`AddHttpClient`, `BaseAddress` de `Catalog:BaseAddress` obrigatório, `Timeout` de 5 s; ids fora da faixa de `int` descartados; não-2xx, `HttpRequestException` e tempo esgotado → `CatalogUnavailableException`) e registrá-lo no `AddInfrastructure`
- [ ] 2.3 Testes do `HttpCatalogProducts` no `Orders.Tests` com um `HttpMessageHandler` falso: 200 vira dicionário por id, produto ausente fica fora, 500 e exceção de rede lançam `CatalogUnavailableException`; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 3. Registro do pedido com os dados do Catalog

- [ ] 3.1 Trocar o `bool Success` do `CreateOrderResult` por `Outcome` (`Created`, `Invalid`, `Unavailable`) com a fábrica `Unavailable()`, ajustando quem o usa; verificar com `dotnet build Scorponok.slnx`
- [ ] 3.2 Reescrever a montagem dos itens no `OrderHandler`: uma chamada ao `ICatalogProducts` antes da transação (só se houver itens), preço/nome/foto do Catalog, erro `Produto {id} não encontrado.` somado à validação do `Order`, `Unavailable` sem tocar no banco
- [ ] 3.3 Mapear `Unavailable` → `503` (`Results.Problem`) no `CreateOrderAsync` do `Orders.Web.Api`
- [ ] 3.4 Atualizar `OrderHandlerTests` e os testes do endpoint: preço adulterado é substituído (total e `PaymentRequested`), nome e foto do Catalog, produto inexistente vira `Invalid` sem transação, Catalog indisponível vira `Unavailable` sem transação nem publicação, `503` no endpoint; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 4. Configuração e verificação ponta a ponta

- [ ] 4.1 `Catalog:BaseAddress` no `appsettings.Development.json` do Orders (`http://localhost:5064`) e, no `docker-compose.yml`, `Catalog__BaseAddress: http://catalog-api:8080` e `depends_on` do `catalog-api` no `orders-api`
- [ ] 4.2 Com o compose no ar: `POST /createOrder` com `unitPrice` 0.01 responde `201` com o total pelo preço do Catalog, e o pagamento é cobrado nesse valor; produto inexistente responde `400` com a mensagem; com `docker compose stop catalog-api`, responde `503` e nenhum pedido é gravado
- [ ] 4.3 Checkout no navegador: a confirmação mostra o total pelo preço do Catalog

## 5. Documentação

- [ ] 5.1 Atualizar o `usecases/UC03-finalizar-pedido.md` (remover a limitação conhecida; fluxos de produto inexistente e Catalog indisponível) e o `CLAUDE.md` (o `POST /createOrder` busca preço, nome e foto no Catalog e depende dele; `Catalog:BaseAddress`; endpoint `GetProductsByIds`)
- [ ] 5.2 Rodar `dotnet test Scorponok.slnx` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas

## Workflow follow-up

- Arquivar esta change (`/opsx:archive`) depois da revisão.
