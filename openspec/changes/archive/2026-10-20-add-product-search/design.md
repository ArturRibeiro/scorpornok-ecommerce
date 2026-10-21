# Design

## Context

Motivação em `proposal.md`; requisitos em `specs/product-search/spec.md`.

- **Catalog**: o `Product` tem `Name`, `Sku`, `PictureUri`, `Price` e `Description`. O `IProductQueries.QueryProducts()` é a consulta ordenada por `Id` que o REST e o GraphQL compartilham, paginada pelo `ToPagedList` com o `PagingModel` (página a partir de 1, até 20 por página, sem erro fora disso). No GraphQL, `products(page, pageSize)` devolve `ProductPage { items, pageInfo }`.
- **Carga**: o `CatalogDbContextSeed` lê o `Seeds/products.json` (281 produtos) só com a tabela vazia, depois do `EnsureCreatedAsync`. Não há migrations, então coluna nova só aparece em banco recriado (volume `catalog_data`).
- **Marca**: os 281 produtos têm a marca no texto da descrição ("Marca: Nike"), e ela coincide com a palavra seguinte a "Tênis" no nome. São 10 marcas (Olympikus 69, Asics 44, Adidas 42, Fila 36, Nike 32, Mizuno 23, New Balance 15, Under Armour 11, Puma 7, Skechers 2). Os preços vão de R$ 199,49 a R$ 2.499,99.
- **Descrição**: textos de 588 a 2.753 caracteres, quase todos de tênis de corrida: "corrida" aparece em 275 das 281 descrições e "amortecimento" em 269, mas "caminhada" (35), "academia" (17) e "trail" (6) só aparecem nelas. A marca está no nome de todos os produtos, então, com estes dados, o grupo "precisa da marca" da ordem quase nunca aparece.
- **Loja**: o `Header` já tem o campo "Buscar produtos..." (desktop) e o botão que abre o campo no celular, sem ação. A home usa o `ProductList` (18 por página, `?page=N`) com o `ProductCard` e o `ProductPagination`.
- **Testes do Catalog**: só os cenários SpecFlow (`Catalog.feature`), contra o `Catalog.Web.Api` e o Postgres do compose, no REST e no `/graphql`.

## Goals / Non-Goals

**Goals:** busca e filtros numa única consulta paginada, com as mesmas regras de página do catálogo e uma ordem previsível; sugestões sobre essa mesma consulta; nenhuma dependência nova no front-end e só a extensão `unaccent` no banco.

**Non-Goals:** índices de busca (281 produtos), cache e mudanças no REST.

## Decisions

1. **Marca como coluna, vinda do `products.json`.** O arquivo ganha o campo `Brand` em cada produto, extraído uma única vez da descrição (e conferido com o nome); a carga a copia para `Product.Brand`. *Alternativa:* extrair da descrição na carga ou na consulta, o que deixaria a marca dependente do formato de um texto livre.
2. **`unaccent` com `ILIKE` nos três campos, sem coluna normalizada.** O termo é dividido em palavras, e cada palavra precisa aparecer em `Name`, `Brand` ou `Description`: `EF.Functions.ILike(EF.Functions.Unaccent(campo), EF.Functions.Unaccent("%palavra%"))`, o que ignora maiúsculas e acentos dos dois lados. Os caracteres `%`, `_` e `\` da palavra são escapados, para valerem como texto. A extensão é declarada com `HasPostgresExtension("unaccent")` no `ApplicationCatalogDbContext`, e o `EnsureCreated` a cria; a imagem `postgres:16` já a traz. O `SearchName` e o `Product.NormalizeForSearch` saem. *Alternativas:* colunas normalizadas para nome e descrição, calculadas no domínio (a decisão anterior, só para o nome), que duplicam a descrição no banco; busca textual (`tsvector`), cujo radical em português não equivale a "contém", que exige uma configuração de busca com `unaccent` criada por SQL e cuja relevância (`ts_rank`) não pesa a raridade das palavras, o que com descrições longas e repetitivas favorece os textos maiores.
3. **Uma consulta de busca no `IProductQueries`, ordenada por grupo.** `SearchProducts(ProductSearch, PagingModel)` aplica termo, marcas e faixa de preço com as regras do `PagingModel`. Os predicados são montados palavra a palavra (expressões combinadas com `AndAlso`/`OrElse`), e a ordem é um `CASE` com três grupos: todas as palavras no nome (0), todas no nome ou na marca (1) e as demais, que precisam da descrição (2); depois, `Id`, para as páginas ficarem estáveis. Sem termo, todos ficam no mesmo grupo, e vale a ordem do catálogo. O `ProductSearch` normaliza a entrada: termo com espaços aparados e limitado a 100 caracteres e 10 palavras (o excedente é ignorado); marcas vazias ignoradas; mínimo maior que o máximo devolve página vazia. `GetBrands()` devolve as marcas distintas em ordem alfabética.
4. **GraphQL, só no Catalog.** `searchProducts(term, brands, minPrice, maxPrice, page, pageSize): ProductPage!` reaproveita o `ProductPage` e o `pageInfo`; `productBrands: [String!]!` lista as marcas. As sugestões usam a mesma consulta, com `pageSize: 5`, nos três campos: são os primeiros resultados da página de resultados. (Uma versão anterior deixava as sugestões só no nome e na marca, com um argumento `inDescription`; "a malha", que só está nas descrições, não sugeria nada e parecia não ter resultado.) O tipo `Product` não muda. O schema é exportado e o gateway recomposto pelo `scripts/compose-gateway.sh`; o `SchemaTests` acusa a composição desatualizada. *Alternativa:* um campo próprio para sugestões, que duplicaria a regra de busca.
5. **Loja.**
   - `lib/catalog.ts`: `searchProducts(filters, page, pageSize, signal)` e `getBrands(signal)`.
   - `components/search/SearchBox`: o campo do cabeçalho (desktop e o painel do celular) com as sugestões, no padrão combobox (`role="combobox"`/`listbox`, setas, Enter e Esc). Consulta a partir de 2 caracteres, com espera de 250 ms entre as teclas e cancelamento da anterior; erro só esconde a lista. Enter (ou "Ver todos os resultados (N)", com o mesmo total da página de resultados) leva a `/search?q=`. Sem sugestões, a lista diz que nenhum produto foi encontrado.
   - Rota `/search` (`SearchPage`): lê `q`, `brands` (separadas por vírgula), `min`, `max` e `page` da query string, que é a fonte de verdade; mudar um filtro volta à página 1. Grade de 18 por página com o `ProductCard`; o `ProductPagination` passa a manter os outros parâmetros da URL ao trocar de página. Filtros numa coluna no desktop e num painel recolhível no celular: marcas em caixas de seleção (de `getBrands`) e preço mínimo e máximo com "Aplicar", que valida a faixa antes de mudar a URL.

## Risks / Trade-offs

- [Banco do Catalog sem as colunas novas] → recriar o volume `catalog_data`, descrito no `CLAUDE.md` e no plano abaixo; os cenários SpecFlow falham com o banco antigo, o que deixa o problema visível.
- [Marca extraída à mão no `products.json`] → a tarefa confere as contagens por marca com a descrição e o nome antes de gravar.
- [`ILIKE '%…%'` com `unaccent` varre a tabela; o `unaccent` não é `IMMUTABLE` e não entra num índice de expressão] → aceitável com 281 produtos; um índice trigram (`pg_trgm`) sobre uma função `unaccent` imutável fica para quando o catálogo crescer.
- [Palavras comuns da descrição, como "corrida", encontram quase o catálogo inteiro] → a ordem por grupo põe na frente os produtos com o termo no nome, inclusive nas sugestões.
- [Banco antigo, sem a extensão] → a busca falha com erro do Postgres; recriar o volume `catalog_data`, como no plano abaixo.
- [Uma consulta por tecla] → espera de 250 ms, mínimo de 2 caracteres e cancelamento da consulta anterior.

## Migration Plan

1. Parar `catalog-api` e `catalog-db`, remover o volume `catalog_data` e subir de novo: a carga cria a extensão `unaccent` e a tabela com `Brand` (sem `SearchName`).
2. Subir o `gateway-api` com os `.far` recompostos e o `frontend` reconstruído.

Rollback: reverter o código e recriar o volume de novo; as colunas a mais não quebram o código antigo, mas a carga só roda com a tabela vazia.
