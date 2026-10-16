# Proposal

## Why

O Orders registra cada item com o preço, o nome e a foto enviados pelo navegador, sem conferir com o Catalog. Quem alterar a requisição compra qualquer produto pelo preço que quiser, e é esse valor que o Payments cobra. O UC03 já registra isso como limitação conhecida.

## What Changes

- **Catalog**: novo endpoint que devolve vários produtos de uma vez, pelos identificadores.
- **Orders**:
  - Ao registrar um pedido, busca no Catalog os produtos dos itens e usa o preço, o nome e a foto de lá. O que vier do navegador nesses campos é ignorado.
  - Item com produto que não existe no Catalog: o pedido é recusado com uma mensagem por produto, como as demais validações.
  - Catalog indisponível: o pedido não é registrado nem cobrado, e a resposta indica indisponibilidade temporária.
  - O total devolvido e cobrado passa a refletir o preço do Catalog no momento do registro.
- **Front-end**: sem mudança de contrato; a tela de confirmação já exibe o total devolvido pelo Orders.
- **Docs**: o UC03 deixa de listar a limitação e ganha os fluxos de produto inexistente e de catálogo indisponível; o `CLAUDE.md` passa a citar a dependência do Orders no Catalog.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `order-placement`: o preço, o nome e a foto dos itens vêm do Catalog; produto inexistente e Catalog indisponível passam a ter resposta definida.

## Impact

- **Catalog**: um endpoint novo de leitura; os existentes não mudam.
- **Orders**: o registro do pedido passa a depender do Catalog estar no ar (antes dependia só do próprio banco). O compose passa a configurar o endereço interno do Catalog para o Orders.
- **Payments**: sem mudanças; continua cobrando o total do pedido.
- **Testes**: handler do pedido com o Catalog simulado; endpoint novo do Catalog.

## Fora do escopo

- Desconto por item: continua ignorado no total.
- Estoque e disponibilidade do produto.
- Avisar o cliente, antes de confirmar, que o preço mudou desde que o produto entrou no carrinho; ele vê o total final na confirmação.
- Réplica local de preços no Orders (eventos do Catalog), que eliminaria a dependência do Catalog no registro.
- Incluir o Catalog no `/health/ready` do Orders.
