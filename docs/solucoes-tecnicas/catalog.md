# Catálogo

A parte da loja que guarda os produtos, com nome, foto, descrição e preço. A solução abaixo está **em funcionamento**. Visão geral em [README.md](README.md).

## 7. Preço conferido no momento da compra

**Problema:** o pedido usa o preço que chega do navegador, sem conferir com o catálogo. Quem alterar o envio consegue comprar qualquer produto pelo preço que quiser, e é esse valor que é cobrado.

**Solução:** o catálogo passa a informar, de uma só vez, o preço, o nome e a foto de todos os produtos de um pedido. A área de pedidos usa esses dados, e não os do navegador. Se o preço mudou desde que o produto entrou no carrinho, vale o preço do catálogo, e o cliente vê o total final na confirmação.

Com isso, registrar um pedido passa a depender do catálogo estar no ar. Se ele estiver fora, a loja avisa que não conseguiu registrar o pedido, e nada é cobrado.

Detalhes técnicos: `use-catalog-price-in-orders`.
