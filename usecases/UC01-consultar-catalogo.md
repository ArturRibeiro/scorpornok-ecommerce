# UC01: Consultar catálogo e buscar produtos

**Ator:** Visitante
**Objetivo:** Ver os produtos à venda, ou encontrar um pelo nome, pela marca ou pelo preço, para escolher o que comprar.

## Fluxo principal

1. O visitante acessa a loja.
2. O sistema exibe os produtos com foto, nome e preço, em páginas.
3. O visitante navega pelas páginas do catálogo.
4. O visitante abre um produto.
5. O sistema exibe a foto, o nome, a descrição e o preço do produto.

## Fluxos alternativos

- **2a. Nenhum produto cadastrado:** o sistema informa que não há produtos.
- **2b. Catálogo indisponível:** o sistema informa o erro e oferece tentar de novo.
- **3a. Página além da última:** o sistema informa que não há produtos nessa página.
- **5a. Produto inexistente:** o sistema informa que o produto não foi encontrado e oferece voltar ao catálogo.

## Fluxo de busca

1. O visitante digita no campo "Buscar produtos" do cabeçalho.
2. A partir de 2 caracteres, o sistema sugere os 5 primeiros produtos encontrados pelo nome, pela marca ou pela descrição, com foto, nome e preço.
3. O visitante escolhe uma sugestão, e o sistema abre o produto (passo 5 do fluxo principal); ou confirma o termo (Enter ou "Ver todos os resultados").
4. O sistema exibe os resultados, com a mesma grade e as mesmas páginas da página inicial, e quantos produtos foram encontrados.
5. O visitante filtra por uma ou mais marcas e por faixa de preço, e o sistema atualiza os resultados a partir da primeira página.

- **2a. Nenhum produto com o termo:** a lista informa que nenhum produto foi encontrado.
- **2b. Catálogo indisponível:** as sugestões não aparecem, sem mensagem de erro.
- **4a. Nenhum resultado:** o sistema informa que nenhum produto foi encontrado e oferece limpar os filtros (mantendo o termo) e voltar à loja.
- **4b. Catálogo indisponível:** o sistema informa o erro e oferece tentar de novo.
- **5a. Preço mínimo maior que o máximo:** o sistema indica a faixa como inválida e não muda os resultados.

## Regras de negócio

- RN01: A página inicial exibe até 18 produtos por página.
- RN02: A página atual fica no endereço, então o visitante pode voltar a ela ou compartilhá-la.
- RN03: Não é preciso estar logado para consultar o catálogo nem para buscar.
- RN04: A busca procura todas as palavras digitadas no nome, na marca e na descrição do produto, em qualquer ordem, sem diferenciar maiúsculas, minúsculas e acentos ("tenis nike" encontra "Tênis Nike"; "caminhada" encontra quem a cita na descrição). As sugestões seguem a mesma regra.
- RN07: Os resultados trazem primeiro os produtos com todas as palavras no nome, depois os que precisam da marca e por último os encontrados só pela descrição; dentro de cada grupo, vale a ordem do catálogo.
- RN05: Os filtros combinam entre si e com o termo: várias marcas valem como "qualquer uma delas", e a faixa de preço inclui os limites. A busca sem termo, só com filtros, também vale.
- RN06: O termo, os filtros e a página ficam no endereço da página de resultados, que pode ser compartilhado.

## Pós-condição

O visitante conhece os produtos disponíveis e pode incluí-los no carrinho (UC02).
