# Tasks

## 1. Marca e nome normalizado no Catalog

- [x] 1.1 Acrescentar o campo `Brand` a cada produto do `Seeds/products.json`, extraído da "Marca:" da descrição; conferir com um script que os 281 têm marca, que ela bate com o nome e que as contagens são as do `design.md` (Olympikus 69 … Skechers 2)
- [x] 1.2 `Product` com `Brand` e `SearchName` (nome em minúsculas e sem acentos, calculado no construtor), colunas na `ProductConfigurations` e a carga copiando a marca; recriar o volume `catalog_data` e conferir no `catalog-db` que as 281 linhas têm `Brand` e `SearchName` (ex.: "tenis nike revolution 8 masculino"); o `SearchName` sai na 6.1

## 2. Consulta de busca e GraphQL

- [x] 2.1 `ProductSearch` (termo aparado, até 100 caracteres e 10 palavras, marcas vazias ignoradas, faixa de preço) e `SearchProducts(ProductSearch, PagingModel)`/`GetBrands()` no `IProductQueries`, sobre o `QueryProducts()` (ordem por `Id`, regras do `PagingModel`; mínimo maior que o máximo → página vazia); verificar com `dotnet build Scorponok.slnx`
- [x] 2.2 `searchProducts(term, brands, minPrice, maxPrice, page, pageSize)` e `productBrands` no `ProductQueries` do GraphQL; exportar o schema e recompor os `.far` com `scripts/compose-gateway.sh`; verificar com `dotnet test src/backend/Gateway/Gateway.Tests` (`SchemaTests`)
- [x] 2.3 Cenários no `Catalog.feature` (com os passos): palavras sem acento e fora de ordem, palavra só da descrição sem resultado, duas marcas sem termo, limites de preço incluídos, faixa invertida vazia, mais de 20 por página vira 20 e a lista de marcas em ordem alfabética; verificar com `dotnet test src/backend/Tests/Ecommerce.Scenarios.Integration.Spec.Tests`
- [x] 2.4 Exemplos no `Gateway.Web.Api.http` (busca, filtros e marcas) e o `CLAUDE.md` da raiz (busca no Catalog e no gateway, `Brand`/`SearchName` e a recriação do `catalog_data`); verificar executando os exemplos contra o gateway no `dotnet run`

## 3. Front-end: sugestões no cabeçalho

- [x] 3.1 `lib/catalog.ts`: tipos dos filtros, `searchProducts(filters, page, pageSize, signal)` e `getBrands(signal)`; verificar com `npm run build && npm run lint` em `src/frontend`
- [x] 3.2 `components/search/SearchBox` no padrão combobox (a partir de 2 caracteres, espera de 250 ms, cancela a anterior, até 5 sugestões com foto, nome e preço, "Ver todos os resultados", "nenhum produto encontrado", erro só esconde a lista; setas, Enter e Esc) e uso no `Header`, no desktop e no painel do celular; verificar no navegador que "nike" sugere 5 produtos, "n" não sugere nada e Enter abre `/search?q=nike`

## 4. Front-end: página de resultados

- [x] 4.1 Rota `/search` com a `SearchPage`: lê `q`, `brands`, `min`, `max` e `page` da URL, grade de 18 com o `ProductCard`, total de encontrados, `LoadingState` e `ErrorState` com tentar de novo; o `ProductPagination` passa a manter os outros parâmetros da URL (a home continua igual); verificar no navegador que abrir `/search?q=nike&max=500&page=2` mostra a mesma página
- [x] 4.2 Filtros (coluna no desktop, painel recolhível no celular): marcas em caixas de seleção e preço mínimo e máximo com "Aplicar", que indica a faixa invertida sem mudar a URL; trocar um filtro volta à página 1; estado sem resultado com "Limpar filtros" e "Voltar à loja"; verificar com `npm run build && npm run lint` e no navegador (Skechers até R$ 200 → nenhum resultado)
- [x] 4.3 Atualizar o `src/frontend/CLAUDE.md` (rota, cliente da busca, `SearchBox`, `SearchPage`, paginação com parâmetros) e o caso de uso `usecases/UC01-consultar-catalogo.md` com a busca e os filtros

## 5. Verificação integrada e docs

- [x] 5.1 No compose, com `catalog_data` recriado e `gateway-api` e `frontend` reconstruídos: sugestões, resultados com marca e preço, endereço compartilhado e `catalog-api` parado (resultado com "Tentar de novo", sugestões somem sem erro); registrar o cenário no `docs/testes/roteiro.md` e o item no `docs/solucoes-tecnicas/catalog.md`
- [x] 5.2 Rodar `dotnet test Scorponok.slnx`, todos sem falhas

## 6. Busca em nome, marca e descrição

- [x] 6.1 Tirar o `SearchName` e o `Product.NormalizeForSearch` (domínio, `ProductConfigurations`) e declarar `HasPostgresExtension("unaccent")` no `ApplicationCatalogDbContext`; recriar o volume `catalog_data` e conferir no `catalog-db` que o `\dx` lista o `unaccent`, que a tabela não tem `searchname` e que as 281 linhas têm `brand`
- [x] 6.2 `SearchProducts`: cada palavra em `ILIKE` sobre `unaccent` de `Name`, `Brand` e `Description` (ou só nome e marca com `InDescription = false`), com `%`, `_` e `\` escapados; ordem pelos três grupos (nome, nome e marca, descrição) e depois `Id`; verificar com `dotnet build Scorponok.slnx`
- [x] 6.3 Argumento `inDescription: Boolean! = true` (opcional) no `searchProducts` do GraphQL; exportar o schema e recompor os `.far` com `scripts/compose-gateway.sh`; verificar com `dotnet test src/backend/Gateway/Gateway.Tests`
- [x] 6.4 `Catalog.feature`: "caminhada" encontrada pela descrição (no lugar de "amortecimento" sem resultado), "asics caminhada", "masculino" com os do nome antes dos da descrição, `inDescription: false` sem "caminhada" e termo com `%` sem erro; verificar com `dotnet test src/backend/Tests/Ecommerce.Scenarios.Integration.Spec.Tests`
- [x] 6.5 `SearchBox` com `inDescription: false`, "Ver todos os resultados" sem contagem e, sem sugestões, a mensagem de que não há sugestões com a opção de buscar o termo em todos os produtos; verificar com `npm run build && npm run lint` e no navegador que "caminhada" não sugere nada, oferece a busca e a página de resultados traz os 35 produtos
- [x] 6.6 Atualizar o `CLAUDE.md` da raiz e o do front-end, o `usecases/UC01-consultar-catalogo.md` (RN04 e a ordem), o cenário 33 do `docs/testes/roteiro.md`, o item 12 do `docs/solucoes-tecnicas/catalog.md` e os exemplos do `Gateway.Web.Api.http`; verificar executando os exemplos contra o gateway
- [x] 6.7 No compose, com `catalog_data` recriado e `catalog-api`, `gateway-api` e `frontend` reconstruídos, repetir "caminhada", "masculino" e as sugestões; rodar `dotnet test Scorponok.slnx`, todos sem falhas

## 7. Sugestões nos três campos

- [x] 7.1 Tirar o `InDescription` do `ProductSearch` e do `ProductSearchExpressions`, e o argumento `inDescription` do `searchProducts`; exportar o schema e recompor os `.far`; verificar com `dotnet test src/backend/Gateway/Gateway.Tests`
- [x] 7.2 `Catalog.feature`: trocar "Sugestões só no nome e na marca" por "a malha" encontrada pela descrição; verificar com `dotnet test src/backend/Tests/Ecommerce.Scenarios.Integration.Spec.Tests`
- [x] 7.3 `SearchBox` e `lib/catalog.ts` sem `inDescription`: sugestões nos três campos, "Ver todos os resultados (N)" com o total e, sem sugestões, "Nenhum produto encontrado"; verificar com `npm run build && npm run lint` e no navegador que "a malha" sugere produtos e "xyz" informa que nenhum foi encontrado
- [x] 7.4 Atualizar o `CLAUDE.md` da raiz e o do front-end, o `usecases/UC01-consultar-catalogo.md`, o cenário 33 do `docs/testes/roteiro.md`, o item 12 do `docs/solucoes-tecnicas/catalog.md` e os exemplos do `Gateway.Web.Api.http`
- [x] 7.5 No compose, com `catalog-api`, `gateway-api` e `frontend` reconstruídos, repetir "a malha" e "xyz" nas sugestões; rodar `dotnet test Scorponok.slnx`, todos sem falhas

## Workflow follow-up

- Arquivar a change (`/opsx:archive`) depois da revisão.
