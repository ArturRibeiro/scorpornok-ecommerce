# Catálogo

A parte da loja que guarda os produtos, com nome, foto, descrição e preço. Cada item indica a sua situação. Visão geral em [README.md](README.md).

## 7. Preço conferido no momento da compra

**Problema:** o pedido usava o preço que chegava do navegador, sem conferir com o catálogo. Quem alterasse o envio conseguia comprar qualquer produto pelo preço que quisesse, e era esse valor que era cobrado.

**Solução:** o catálogo passa a informar, de uma só vez, o preço, o nome e a foto de todos os produtos de um pedido. A área de pedidos usa esses dados, e não os do navegador. Se o preço mudou desde que o produto entrou no carrinho, vale o preço do catálogo, e o cliente vê o total final na confirmação.

Com isso, registrar um pedido passa a depender do catálogo estar no ar. Se ele estiver fora, a loja avisa que não conseguiu registrar o pedido, e nada é cobrado.

**Situação:** em funcionamento.

Detalhes técnicos: `use-catalog-price-in-orders` (arquivada em `openspec/changes/archive/`).
