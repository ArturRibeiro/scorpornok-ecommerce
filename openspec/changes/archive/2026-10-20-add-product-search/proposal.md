# Proposal

## Why

O cliente só encontra um produto folheando as 16 páginas da home: o campo "Buscar produtos" do cabeçalho existe, mas não faz nada. Com 281 tênis de 10 marcas e preços de R$ 199 a R$ 2.500, procurar por nome, marca e faixa de preço é o caminho natural para chegar ao produto.

## What Changes

- Busca no nome, na marca e na descrição do produto, sem diferenciar maiúsculas, minúsculas e acentos ("tenis nike" acha "Tênis Nike"; "caminhada" acha quem a cita na descrição).
- Resultados em ordem de relevância por campo: primeiro quem tem o termo no nome, depois na marca e por último só na descrição.
- Sugestões enquanto o cliente digita, com a mesma busca da página de resultados: os primeiros produtos, com foto, nome e preço, que levam direto à página do produto.
- Página de resultados com a mesma grade e paginação da home; o termo e os filtros ficam no endereço, para voltar e compartilhar.
- Filtros por marca e por faixa de preço, combináveis entre si e com o termo; a busca sem termo, só com filtros, também vale.
- A marca passa a ser um dado próprio do produto, em vez de aparecer só no texto da descrição.
- Busca sem resultado mostra uma mensagem clara e um caminho de volta para a loja; com o catálogo fora, a mensagem de sempre, com "Tentar de novo".
- A busca passa pelo ponto de entrada único, como o resto do catálogo, e não exige login.

## Capabilities

### New Capabilities
- `product-search`: busca de produtos por nome, marca e descrição, sugestões ao digitar, filtros por marca e preço e a página de resultados.

### Modified Capabilities
- Nenhuma.

## Impact

- Catalog: marca no produto e na carga inicial, consulta de busca e de marcas, no GraphQL.
- Gateway: composição recomposta com o novo schema do Catalog.
- Front-end: campo de busca do cabeçalho (desktop e celular), sugestões e página de resultados com os filtros.
- Ambiente: o banco do Catalog precisa ser recriado, para a marca e para a extensão que ignora acentos.
- Docs, casos de uso e roteiro de testes.

## Fora do escopo

- Busca no SKU ou por erros de digitação (busca aproximada) e por radicais ("corredor" achar "corrida").
- Ordenação escolhida pelo cliente (preço) e outros filtros (gênero, tamanho, categoria).
- Busca e filtros no REST do Catalog.
- Histórico de buscas e buscas mais populares.
- Mostrar a marca na página do produto.
