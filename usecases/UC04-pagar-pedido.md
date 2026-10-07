# UC04: Pagar pedido

**Atores:** Cliente, operadora de cartão
**Objetivo:** Cobrar o pedido no cartão de crédito e confirmar a compra.

## Pré-condição

Existe um pedido **Pendente** (UC03).

## Fluxo principal

1. O sistema envia a cobrança à operadora de cartão.
2. A operadora aprova o pagamento.
3. O sistema muda o pedido para **Confirmado**.
4. O sistema informa ao cliente que a compra foi confirmada.

## Fluxos alternativos

- **2a. Pagamento recusado:** o sistema muda o pedido para **Falha no pagamento** e avisa o cliente.
- **2b. Operadora indisponível:** o pedido continua **Pendente** e a cobrança é tentada novamente.

## Regras de negócio

- RN01: O valor cobrado é o total do pedido.
- RN02: O pagamento é feito à vista ou parcelado no cartão de crédito.
- RN03: Só é possível pagar pedidos **Pendentes**, para evitar cobrança em dobro.
- RN04: Os dados do cartão não são armazenados pela loja.

## Pós-condição

Pedido **Confirmado** ou com **Falha no pagamento**.
