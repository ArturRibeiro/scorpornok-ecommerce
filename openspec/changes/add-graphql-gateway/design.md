# Design

## Context

Motivação em `proposal.md`; requisitos em `specs/`.

- Catalog: `GET /Products` (`PagingModel`, até 20, ordem por `Id`, `IPagedList`), `GET /GetProductById/{id}` e `GET /GetProductsByIds` (usado pelo Orders), todos sobre o `IProductQueries`.
- Loja: `getProducts`/`getProduct` em `lib/catalog.ts`, com `VITE_CATALOG_API_URL`; a home usa paginação numerada.
- Fusion 16: subgraphs com `HotChocolate.AspNetCore` (+ `CommandLine` e `Types.Analyzers`), gateway com `HotChocolate.Fusion.AspNetCore`, composição pelo `nitro fusion compose` num arquivo `.far`.

## Goals / Non-Goals

**Goals:** definir o padrão das próximas APIs; GraphQL e REST sobre o mesmo `IProductQueries`.

**Non-Goals:** autenticação, mutations, subscriptions e o Nitro (registry).

## Decisions

1. **Subgraph no `Catalog.Web.Api`**: `/graphql` ao lado do REST, em `GraphQL/`. *Alternativa:* projeto separado, que seria só mais um processo.
2. **Schema**: `products(page, pageSize)` sobre o `GetAllProducts(PagingModel)` do REST, com o mesmo `pageInfo`, para manter as páginas numeradas e os mesmos ajustes (tamanho acima de 20 vira 20); `productById` com `[Lookup]`, nulo se não existe, já pronto para o Orders. *Alternativas:* `UseOffsetPaging`, descartado porque responde erro (`HC0051`) acima do limite em vez de ajustar; cursor, que obrigaria a trocar a paginação da home.
3. **Composição versionada**: `schema.graphqls`, `schema-settings.json` e os `.far` no repositório, gerados por `scripts/compose-gateway.sh`; um teste falha se o schema mudar sem recompor. *Alternativa:* compor no `Dockerfile`, o que exigiria o Nitro e o Catalog no ar durante o build.
4. **Endereço do subgraph por ambiente**: o `schema-settings.json` usa `{{CATALOG_URL}}` e `environments` (`local`: `localhost:5064`; `compose`: `catalog-api:8080`), e o `nitro fusion compose --env` grava o endereço no `.far`. Ficam dois arquivos, `gateway.local.far` e `gateway.compose.far`, e o gateway carrega o de `Fusion:Archive` (padrão `gateway.local.far`; o compose aponta o outro). Confirmado num projeto descartável com o Fusion 16.6.9.
5. **Gateway**: `src/backend/Gateway/Gateway.Web.Api`, `Dockerfile` genérico, porta 5200; CORS de `Cors:AllowedOrigins`; `GraphQL:MaxDepth`/`MaxCost`; introspecção só em Development; `/health/live` e `/health/ready` (só o `.far`); timeout de 5 s no subgraph.
6. **Loja**: `lib/graphql.ts` mínimo sobre `fetch`, sem cliente GraphQL (urql/Apollo vêm com as subscriptions); `lib/catalog.ts` mantém as assinaturas e recebe o mesmo `pageInfo` do REST; `VITE_GRAPHQL_URL` substitui `VITE_CATALOG_API_URL`.

## Risks / Trade-offs

- [Fusion 16 recente, APIs mudaram desde a 15] → versões fixas e só a documentação da 16.
- [`.far` desatualizado] → teste de schema e script documentado.
- [Mais um serviço no caminho da home] → gateway fora mostra o `ErrorState`, como o Catalog fora hoje.

## Migration Plan

Subir `catalog-api`, `gateway-api` e o `frontend` reconstruído; sem mudança de banco. Rollback: reverter e reconstruir o `frontend` com o `VITE_CATALOG_API_URL`.
