# Proposal

## Why

O ponto de entrada GraphQL só atende o catálogo, que não exige login. O cadastro do cliente é a primeira API autenticada a migrar e define como o cliente é identificado e como os erros chegam à loja pelo gateway, padrão que o Orders vai reaproveitar.

## What Changes

- Gateway identifica o cliente pelo token da conta e o repassa às APIs.
- Customers como subgraph: ler e salvar o próprio cadastro, com os erros por campo de hoje.
- "Meu cadastro" e o preenchimento do checkout passam pelo gateway; nada muda na tela.
- REST do Customers continua por enquanto.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `graphql-gateway`: cliente identificado no ponto de entrada e cadastro por ele.

## Impact

- Gateway: autenticação e repasse do token; Keycloak ganha o audience do gateway.
- Customers: schema GraphQL ao lado do REST.
- Front-end: cliente do cadastro passa a usar o gateway.
- Docs: `CLAUDE.md` e `docs/`.

## Fora do escopo

- Orders (change `add-orders-subgraph`).
- Remover o REST do Customers.
- Autorização por papel ou por campo além da regra de hoje (cliente com e-mail confirmado).
