# Tasks

## 1. Autenticação no gateway

- [x] 1.1 Audience `gateway-api` no realm (`gateway-api-audience`) e no Keycloak em execução; verificar o `aud` de um token novo
  - Mapper `gateway-api-audience` no `scorponok-realm.json` e criado no Keycloak em execução (`kcadm`, client `scorponok-web`). O realm está sem usuários agora; o `aud` de um token real é conferido no teste do navegador (3.3).
- [x] 1.2 JwtBearer no gateway (seção `Keycloak`) e o handler que repassa o `Authorization` ao cliente `fusion`; verificar com `dotnet build`
- [x] 1.3 `Gateway.Tests`: token inválido → `401`, token repassado ao subgraph, catálogo sem token; verificar com `dotnet test src/backend/Gateway/Gateway.Tests`

## 2. Customers

- [x] 2.1 `GraphQL/` no `Customers.Web.Api`: `me` e `updateMyProfile` com `[Authorize]`, payload com `FieldError`; verificar pelo `Customers.Web.Api.http`
- [x] 2.2 `schema export` e `schema-settings.json` do `customers` no script; recompor os `.far`
- [x] 2.3 `Customers.Tests`: sem login, sem e-mail confirmado, erros por campo e cadastro do dono; `SchemaTests` com o Customers; verificar com `dotnet test`

## 3. Compose e loja

- [x] 3.1 Compose: Keycloak e `depends_on` do `customers-api` no `gateway-api`; verificar `me` com token em `http://localhost:5200/graphql`
  - `customers-api` e `gateway-api` reconstruídos: `me` sem token → `AUTH_NOT_AUTHENTICATED` vindo do Customers, token inválido → `401`, catálogo sem token normal. `me` com token válido fica para o 3.3 (realm sem usuários).
- [x] 3.2 `graphqlRequest` com token e `SessionExpiredError`; `lib/customers.ts` pelo gateway; sem `VITE_CUSTOMERS_API_URL`; verificar com `npm run build && npm run lint`
- [x] 3.3 No navegador: "Meu cadastro" (ler, salvar, CPF inválido), checkout preenchido e sessão expirada
  - Testado no navegador pelo usuário em 2026-10-19 (cenário 30 do roteiro), com conta real.

## 4. Docs e testes

- [x] 4.1 `CLAUDE.md` (raiz e front-end), `docs/` e `.http`
- [x] 4.2 `dotnet test Scorponok.slnx` sem falhas

## Workflow follow-up

- Arquivar com `/opsx:archive` antes de aplicar a `add-orders-subgraph`.
