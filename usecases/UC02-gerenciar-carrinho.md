# UC02: Gerenciar carrinho

**Ator:** Visitante
**Objetivo:** Separar os produtos que deseja comprar e acompanhar o valor total.

## Pré-condição

O visitante está na página de um produto (UC01).

## Fluxo principal

1. O visitante escolhe a quantidade e adiciona o produto ao carrinho.
2. O sistema inclui o produto e atualiza o contador do carrinho no cabeçalho.
3. O visitante abre o carrinho.
4. O sistema exibe cada item com foto, nome, preço unitário, quantidade e subtotal, e o total.
5. O visitante segue para finalizar o pedido (UC03).

## Fluxos alternativos

- **1a. Produto já está no carrinho:** o sistema soma a nova quantidade à existente.
- **1b. Comprar agora:** o sistema adiciona o produto e abre o carrinho em seguida.
- **4a. Alterar quantidade:** o visitante aumenta ou diminui a quantidade de um item, e o sistema recalcula o subtotal e o total.
- **4b. Remover produto:** o visitante remove um item, e o sistema recalcula o total.
- **4c. Carrinho vazio:** o sistema informa que o carrinho está vazio e oferece voltar ao catálogo.

## Regras de negócio

- RN01: A quantidade de cada item é no mínimo 1.
- RN02: O total é a soma de preço unitário × quantidade de cada item, sem frete nem impostos.
- RN03: O carrinho fica guardado no navegador: não exige login, sobrevive a recarregar a página e não acompanha o visitante em outro navegador ou aparelho.
- RN04: Não é possível finalizar um carrinho vazio.

## Pós-condição

O carrinho contém os itens que o visitante deseja comprar.
