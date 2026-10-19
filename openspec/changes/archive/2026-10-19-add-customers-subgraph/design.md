# Design

## Context

Motivação em `proposal.md`; requisitos em `specs/`.

- Customers: `GET`/`PUT /customers/me` com a política `VerifiedCustomer` (JwtBearer, audience `customers-api`), `GetCustomerProfileQuery` e `UpdateCustomerProfileCommand` (erros por campo nas chaves do JSON: `cpf`, `address.zipCode`).
- Gateway (`add-graphql-gateway`): sem autenticação; cliente HTTP `fusion` para os subgraphs; composição pelo `scripts/compose-gateway.sh`.
- Keycloak: o client `scorponok-web` põe `orders-api` e `customers-api` no audience.
- Loja: `getProfile`/`saveProfile` em `lib/customers.ts`, usados pela `ProfilePage` e pela `CheckoutPage`.

## Goals / Non-Goals

**Goals:** padrão de autenticação e de erros que o Orders reaproveita; o subgraph continua dono da regra de acesso.

**Non-Goals:** confiar no gateway sem validar no subgraph; mudar regras do cadastro.

## Decisions

1. **Token validado nos dois lados**: o gateway valida o JWT (audience `gateway-api`, mesmos `Authority`, `ClockSkew` e `MapInboundClaims=false` das APIs) e recusa credencial inválida com `401`; sem credencial, a consulta segue (o catálogo é público). O subgraph valida de novo com o seu audience. *Alternativa:* só o gateway valida e repassa claims em cabeçalhos; descartado (escolha do usuário): um subgraph exposto aceitaria qualquer cabeçalho.
2. **Repasse do token**: um `DelegatingHandler` no cliente `fusion` copia o `Authorization` da requisição atual (`IHttpContextAccessor`). Vale para todos os subgraphs.
3. **Audience `gateway-api`**: novo mapper `gateway-api-audience` no `scorponok-web` do `keycloak/scorponok-realm.json`. Num realm já importado, adicionar pelo console (como o `customers-api-audience`), sem recriar o volume.
4. **Acesso no subgraph**: `[Authorize(Policy = VerifiedCustomer)]` do Hot Chocolate (`HotChocolate.AspNetCore.Authorization`) nos campos do cadastro; sem login → `AUTH_NOT_AUTHENTICATED`, sem e-mail confirmado → `AUTH_NOT_AUTHORIZED`, com `data` nulo no campo.
5. **Schema**: `me: CustomerProfile!` e `updateMyProfile(input: UpdateMyProfileInput!): UpdateMyProfilePayload!`, com `payload { profile, errors: [FieldError!]! }` e `FieldError { field, messages }` (chaves de hoje). *Alternativa:* convenções de mutation com `[Error]`, que exigiriam exceções para validação; o payload explícito vira o padrão do Orders.
6. **Loja**: `graphqlRequest` ganha o token opcional e converte `401` ou `AUTH_NOT_AUTHENTICATED` em `SessionExpiredError`; `lib/customers.ts` mantém as assinaturas. `VITE_CUSTOMERS_API_URL` sai da loja.
7. **Composição**: o script ganha `customers-api` (`environments`: `localhost:5230` e `customers-api:8080`); o `SchemaTests` passa a cobrir os dois subgraphs.

## Risks / Trade-offs

- [Realm já importado sem o audience novo] → o gateway recusa o token; passo no `CLAUDE.md` para adicionar pelo console.
- [Token validado duas vezes] → custo baixo (chaves em cache) em troca de o subgraph não depender do gateway.
- [Compose: gateway depende do customers-api] → `depends_on` com `service_started`; com o Customers fora, só o campo do cadastro falha.

## Migration Plan

Adicionar o audience no realm, subir `customers-api`, `gateway-api` e o `frontend` reconstruído. Rollback: reverter e reconstruir o `frontend` com o `VITE_CUSTOMERS_API_URL` (o REST continua).
