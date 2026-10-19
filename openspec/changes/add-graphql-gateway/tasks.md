# Tasks

## 1. Base do Fusion

- [x] 1.1 Versões do Hot Chocolate e do Fusion 16 no `Directory.Build.props` e `dotnet-tools.json` com o Nitro; verificar com `dotnet tool restore` e `dotnet build Scorponok.slnx`
- [x] 1.2 Resolver a Open Question (endereço por ambiente) e atualizar a Decisão 4

## 2. Catalog

- [x] 2.1 Consulta ordenada compartilhada no `IProductQueries`; verificar com o `Catalog.feature` atual
- [x] 2.2 `GraphQL/` com `products(page, pageSize)` (regras do REST) e `productById` (`[Lookup]`); verificar pelo `Catalog.Web.Api.http`
- [x] 2.3 Exportar e versionar `schema.graphqls` e `schema-settings.json`
- [x] 2.4 Cenários no `Catalog.feature`: primeira página, acima de 20, produto inexistente, mesma página nas duas APIs; verificar com `dotnet test src/backend/Tests/Ecommerce.Scenarios.Integration.Spec.Tests`

## 3. Gateway

- [x] 3.1 `Gateway.Web.Api` na solution: `.far` de `Fusion:Archive`, CORS, limites, introspecção só em Development, health e timeout
- [x] 3.2 `scripts/compose-gateway.sh` e os `.far` (local e compose) versionados; verificar consultando produtos pelo gateway (`Gateway.Web.Api.http`)
- [x] 3.3 `Gateway.Tests`: outra origem bloqueada, limite, introspecção, pronto sem o Catalog, erro com o Catalog fora e schema versionado atualizado; verificar com `dotnet test`

## 4. Compose e loja

- [x] 4.1 Serviço `gateway-api` (porta 5200); verificar consultando `http://localhost:5200/graphql`
- [x] 4.2 `lib/graphql.ts`, `lib/catalog.ts` sobre o gateway e `VITE_GRAPHQL_URL`; verificar com `npm run build && npm run lint`
- [x] 4.3 No compose: home, paginação e produto como antes, sem chamadas ao `localhost:5064`; com o Catalog parado, `ErrorState`
  - Em 2026-10-19, `catalog-api`, `gateway-api` e `frontend` reconstruídos no compose: o bundle da loja só aponta para `localhost:5200/graphql` (nenhuma referência ao `localhost:5064`); as consultas da home (página 1 com 18: 18 itens, 16 páginas) e do produto (existente e inexistente) responderam pelo gateway com a origem da loja; com o Catalog parado, o gateway devolve `errors` no campo, o que leva a loja ao `ErrorState`; o `GetProductsByIds` usado pelo Orders continua respondendo. Conferido no navegador pelo usuário em 2026-10-19 (grade, paginação, página de produto e `ErrorState`).

## 5. Docs e testes

- [x] 5.1 `CLAUDE.md` (raiz e front-end), compose e UC01
- [x] 5.2 `dotnet test Scorponok.slnx` sem falhas

## Workflow follow-up

- Arquivar com `/opsx:archive` depois da revisão.
- Próximas: Customers (com autenticação no gateway), Orders (subscription no lugar do SignalR) e remoção do REST sem uso.
