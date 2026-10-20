# Tasks

## 1. Payments e gateway

- [x] 1.1 `GraphQL/` no `Payments.Web.Api`: `tokenizeCard` com `CardFieldError`; schema exportado e `payments` no script; `.far` recompostos
- [x] 1.2 Proteções no gateway (`IncludeExceptionDetails`, `CollectOperationPlanTelemetry`) e `depends_on` do `payment-api` no compose
- [x] 1.3 Testes: mutation no `Payments.Tests` (token e erros por campo), `SchemaTests` com o Payments e teste do gateway sem vazamento do cartão em log e resposta; verificar com `dotnet test`

## 2. Loja

- [x] 2.1 `lib/payments.ts` pelo gateway, sem `VITE_PAYMENTS_API_URL`; verificar com `npm run build && npm run lint`
- [x] 2.2 No navegador: compra com cartão válido, CVV inválido e nova tentativa, com a tokenização pelo `localhost:5200/graphql`

## 3. Docs

- [x] 3.1 `CLAUDE.md` (raiz e front-end), `docs/` e `arquitetura.drawio`
- [x] 3.2 `dotnet test Scorponok.slnx` sem falhas
