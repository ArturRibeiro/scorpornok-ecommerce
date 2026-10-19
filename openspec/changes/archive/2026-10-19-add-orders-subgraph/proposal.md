# Proposal

## Why

Com o catálogo e o cadastro no ponto de entrada GraphQL, os pedidos são a última API que a loja chama direto. Esta change passa registro, nova tentativa e histórico para o gateway, com o padrão de identificação e de erros da `add-customers-subgraph`.

## What Changes

- Orders como subgraph: registrar pedido, pagar de novo com outro cartão e consultar os próprios pedidos.
- Desfechos de hoje (pedido inválido, catálogo fora, pedido não encontrado, pagamento que não pode ser refeito) viram erros com código, sem mudar as mensagens.
- Checkout, nova tentativa e "Meus pedidos" passam pelo gateway; nada muda na tela.
- Acompanhamento do pagamento continua pelo hub de hoje; REST do Orders continua.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `graphql-gateway`: pedidos pelo ponto de entrada e os seus erros.

## Impact

- Orders: schema GraphQL ao lado do REST e do hub.
- Gateway: composição com o Orders.
- Front-end: cliente do Orders passa a usar o gateway, menos o hub.
- Docs: `CLAUDE.md` e `docs/`.

## Fora do escopo

- Subscription no lugar do SignalR (change própria, com cliente GraphQL).
- Tokenização do cartão (continua REST no Payments).
- Remover o REST do Orders.
- Produto atual do catálogo dentro do pedido (o histórico mostra o que foi comprado).

## Dependências

- `add-customers-subgraph` aplicada e arquivada antes (autenticação no gateway).
