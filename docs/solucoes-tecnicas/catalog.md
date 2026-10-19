# Catálogo

A parte da loja que guarda os produtos, com nome, foto, descrição e preço. Cada item indica a sua situação. Visão geral em [README.md](README.md).

## 7. Preço conferido no momento da compra

**Problema:** o pedido usava o preço que chegava do navegador, sem conferir com o catálogo. Quem alterasse o envio conseguia comprar qualquer produto pelo preço que quisesse, e era esse valor que era cobrado.

**Solução:** o catálogo passa a informar, de uma só vez, o preço, o nome e a foto de todos os produtos de um pedido. A área de pedidos usa esses dados, e não os do navegador. Se o preço mudou desde que o produto entrou no carrinho, vale o preço do catálogo, e o cliente vê o total final na confirmação.

Com isso, registrar um pedido passa a depender do catálogo estar no ar. Se ele estiver fora, a loja avisa que não conseguiu registrar o pedido, e nada é cobrado.

**Situação:** em funcionamento.

Detalhes técnicos: `use-catalog-price-in-orders` (arquivada em `openspec/changes/archive/`).

## 11. Catálogo pelo ponto de entrada único

**Problema:** a loja consultava cada parte do sistema por um endereço próprio, com formatos e erros diferentes. Juntar dados de várias partes numa tela exigia várias chamadas, e não havia proteção contra consultas grandes demais.

**Solução:**
- A loja ganha um ponto de entrada único (GraphQL), que encaminha cada consulta à parte dona dos dados. O catálogo é o primeiro a passar por ele: a lista de produtos, com as mesmas páginas de antes (até 20 produtos), e os dados de um produto.
- O catálogo responde igual pelos dois caminhos: os mesmos produtos, na mesma ordem e com os mesmos preços. O caminho antigo continua, porque o registro de pedidos o usa para conferir os preços.
- Uma consulta grande demais (profunda demais ou com campos demais) é recusada logo na entrada, sem chegar ao catálogo. A descrição completa das consultas disponíveis só aparece em ambiente de desenvolvimento.
- Com o catálogo fora do ar, o ponto de entrada continua no ar e responde com erro só para as consultas do catálogo.

**Situação:** em funcionamento (1ª etapa). Cadastro do cliente e pedidos passam pelo ponto de entrada nas próximas etapas.

Detalhes técnicos: `add-graphql-gateway` (em `openspec/changes/`).
