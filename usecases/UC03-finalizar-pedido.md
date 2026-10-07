# UC03: Finalizar pedido

**Ator:** Cliente
**Objetivo:** Transformar o carrinho em um pedido de compra.

## Pré-condição

O carrinho tem pelo menos um item (UC02).

## Fluxo principal

1. O cliente solicita a finalização da compra.
2. O cliente informa o endereço de entrega.
3. O cliente informa os dados do cartão de crédito e o número de parcelas.
4. O sistema confere os produtos, os preços e o total.
5. O sistema registra o pedido com a situação **Pendente** e gera o número do pedido.
6. O sistema segue para o pagamento (UC04).

## Fluxos alternativos

- **2a. Endereço incompleto:** o sistema solicita os dados que faltam.
- **4a. Produto indisponível ou preço alterado:** o sistema avisa o cliente e volta ao carrinho.

## Regras de negócio

- RN01: O endereço de entrega exige rua, cidade, estado, país e CEP.
- RN02: O pedido pertence ao cliente que o finalizou.
- RN03: O preço considerado é o do catálogo no momento da finalização.
- RN04: A data do pedido é a data da finalização.
- RN05: O pedido só é confirmado após o pagamento aprovado.

## Pós-condição

Pedido registrado como **Pendente**, aguardando pagamento.
