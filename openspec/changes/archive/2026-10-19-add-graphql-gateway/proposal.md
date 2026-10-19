# Proposal

## Why

A loja fala com quatro APIs REST, cada uma com URL, erros e cliente próprios. Esta change é o primeiro passo para um ponto de entrada GraphQL único, validado com o Catalog, que é só leitura e não exige login.

## What Changes

- Novo gateway GraphQL federado, ponto de entrada único da loja.
- Catalog como primeiro subgraph: listagem paginada e produto por id, com as regras de hoje.
- Home e página de produto leem o catálogo pelo gateway; nada muda na tela.
- Gateway com limite de profundidade e custo, e introspecção só em desenvolvimento.
- REST do Catalog continua (o Orders o usa).

## Capabilities

### New Capabilities
- `graphql-gateway`: ponto de entrada único, origens, limites, saúde e API fora do ar.
- `product-catalog`: listagem paginada e produto por id, no GraphQL e no REST.

### Modified Capabilities
- Nenhuma.

## Impact

- Backend: novo serviço de gateway; Catalog expõe GraphQL além do REST.
- Front-end: cliente do catálogo passa a usar o gateway.
- Compose e docs (`CLAUDE.md`, UC01).

## Fora do escopo

- Customers, Orders e Payments no GraphQL; o SignalR continua.
- Autenticação no gateway.
- Tokenização do cartão (continua REST no Payments).
- Remover o REST do Catalog.
- Cache HTTP e consultas persistidas.
