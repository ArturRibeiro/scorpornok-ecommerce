# UC02: Gerenciar carrinho

**Ator:** Cliente
**Objetivo:** Separar os produtos que deseja comprar e acompanhar o valor total.

## Pré-condição

O cliente consultou o catálogo (UC01).

## Fluxo principal

1. O cliente adiciona um produto ao carrinho e informa a quantidade.
2. O sistema inclui o produto e atualiza o valor total.
3. O cliente consulta o carrinho: produtos, quantidades e total.
4. O cliente segue para finalizar o pedido (UC03).

## Fluxos alternativos

- **1a. Produto já está no carrinho:** o sistema soma a nova quantidade à existente.
- **3a. Remover produto:** o cliente remove um item e o sistema atualiza o total.
- **3b. Alterar quantidade:** o cliente muda a quantidade e o sistema atualiza o total.

## Regras de negócio

- RN01: A quantidade de cada item deve ser maior que zero.
- RN02: O total é a soma de preço × quantidade de cada item, menos os descontos.
- RN03: Não é possível finalizar um carrinho vazio.

## Pós-condição

O carrinho contém os itens que o cliente deseja comprar.
