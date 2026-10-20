# Design

## Context

Motivação em `proposal.md`; requisitos em `specs/`.

- Payments: `POST /card-tokens` (sem login, CORS da loja) sobre o `CreateCardTokenCommand` (o `ToString()` não mostra o cartão), com erros por campo do formulário (`cardNumber`, `cvv`, ...).
- Gateway: subgraphs Catalog, Customers e Orders; token repassado pelo `AuthorizationForwardingHandler`.
- Loja: `tokenizeCard` em `lib/payments.ts`, usado pelo checkout e pela nova tentativa.

## Goals / Non-Goals

**Goals:** loja só com o gateway (fora o hub); o cartão não fica registrado no gateway.

**Non-Goals:** exigir login; mudar as regras de validação.

## Decisions

1. **Decisão de segurança**: a tokenização passa pelo gateway, por escolha do usuário, aceitando que o cartão atravesse mais um componente. *Alternativa (a de antes):* manter a loja chamando o Payments direto, para o cartão tocar só o Payments.
2. **Schema**: subgraph `payments` com `tokenizeCard(input: TokenizeCardInput!): TokenizeCardPayload!` (`token { token last4 expiresAt }` ou `errors: [CardFieldError { field message }]`). `CardFieldError`, e não o `FieldError` do Customers, para não compartilhar tipo entre subgraphs com formatos diferentes.
3. **Sem login**: a mutation é pública, como o REST; o token sozinho não cobra nada.
4. **Proteções no gateway**: `IncludeExceptionDetails = false` e `CollectOperationPlanTelemetry = false` explícitos; nenhum log de requisição (HttpLogging) ligado; um teste que tokeniza com o subgraph falhando e confere que o número e o CVV não aparecem nos logs capturados nem na resposta.
5. **Loja**: `tokenizeCard` mantém a assinatura sobre o `graphqlRequest`; sai o `VITE_PAYMENTS_API_URL`.

## Risks / Trade-offs

- [Cartão em mais um componente] → proteções e teste acima; o teste falha se uma configuração futura passar a registrar as variáveis.
- [Gateway fora impede tokenizar] → hoje o checkout já depende do gateway para registrar o pedido.

## Migration Plan

Subir `payment-api`, `gateway-api` e o `frontend` reconstruído. Rollback: reverter e reconstruir o `frontend` (o REST continua).
