# UC01: Consultar catálogo

**Ator:** Visitante
**Objetivo:** Ver os produtos à venda para escolher o que comprar.

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

## Regras de negócio

- RN01: A página inicial exibe até 18 produtos por página.
- RN02: A página atual fica no endereço, então o visitante pode voltar a ela ou compartilhá-la.
- RN03: Não é preciso estar logado para consultar o catálogo.

## Pós-condição

O visitante conhece os produtos disponíveis e pode incluí-los no carrinho (UC02).
