# Catálogo

A parte da loja que guarda os produtos, com nome, foto, descrição e preço. Cada item indica a sua situação.

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

**Situação:** em funcionamento. O cadastro do cliente e os pedidos também passam pelo ponto de entrada (veja [Cadastro do cliente](customers.md) e [Pedidos](orders.md)).

Detalhes técnicos: `add-graphql-gateway` (arquivada em `openspec/changes/archive/`).

## 12. Busca de produtos

**Problema:** o cliente só encontrava um produto folheando as páginas da loja. O campo "Buscar produtos" do topo existia, mas não fazia nada, e não havia como ver só uma marca ou uma faixa de preço.

**Solução:**
- O campo de busca procura o produto pelo nome, pela marca e pela descrição, sem diferenciar maiúsculas, minúsculas e acentos, com as palavras em qualquer ordem: "tenis nike" encontra "Tênis Nike", e "caminhada" encontra os tênis que a citam na descrição.
- Os resultados trazem primeiro quem tem o termo no nome, depois na marca e por último só na descrição, para que palavras comuns nas descrições, como "corrida", não escondam o que o cliente procurou.
- Enquanto o cliente digita, aparecem as 5 primeiras sugestões da mesma busca, com foto, nome e preço, que levam direto ao produto, e a opção de ver todos os resultados.
- A página de resultados tem a mesma grade e as mesmas páginas da loja e filtros por marca e faixa de preço, que se combinam entre si e com o termo. O endereço guarda a busca, então dá para voltar a ela ou compartilhá-la.
- A marca passa a ser um dado próprio de cada produto, em vez de aparecer só no texto da descrição.
- Sem resultado, a loja diz que nada foi encontrado e oferece limpar os filtros. Com o catálogo fora do ar, a página oferece tentar de novo, e as sugestões apenas não aparecem.

**Situação:** em funcionamento.

Detalhes técnicos: `add-product-search` (em `openspec/changes/`).
