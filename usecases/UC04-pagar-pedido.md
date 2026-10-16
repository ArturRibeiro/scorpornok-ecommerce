# UC04: Pagar pedido

**Atores:** Cliente, serviço de pagamentos
**Objetivo:** Cobrar o pedido no cartão de crédito e informar o resultado ao cliente.

## Pré-condição

Existe um pedido **Pendente** (UC03).

## Fluxo principal

1. O sistema solicita a cobrança do total do pedido ao serviço de pagamentos.
2. O serviço de pagamentos aprova o pagamento e o registra.
3. O sistema muda o pedido para **Confirmado** e guarda o identificador do pagamento.
4. O sistema informa ao cliente, na tela da finalização, que a compra foi confirmada.
5. O sistema envia ao e-mail do pedido uma mensagem com o número do pedido, o total e a confirmação.

## Fluxos alternativos

- **2a. Pagamento recusado:** o sistema muda o pedido para **Falha no pagamento**, informa o cliente na tela e envia o e-mail com a recusa.
- **2b. Serviço de pagamentos indisponível:** o pedido continua **Pendente**; a cobrança é feita quando o serviço voltar, sem nova ação do cliente.
- **2c. Mesma cobrança solicitada de novo:** o serviço de pagamentos não cobra outra vez e repete o resultado já registrado.
- **4a. Resultado demora mais de 10 segundos:** o sistema informa que o pagamento está pendente e que o resultado será enviado por e-mail.
- **4b. Cliente saiu da loja:** o resultado chega só pelo e-mail.
- **5a. Servidor de e-mail indisponível:** a situação do pedido é atualizada mesmo assim, e o e-mail é enviado quando o servidor voltar.

## Regras de negócio

- RN01: O valor cobrado é o total calculado pelo sistema para o pedido.
- RN02: O pagamento é feito à vista ou parcelado no cartão de crédito.
- RN03: Só pedidos **Pendentes** mudam de situação; um resultado repetido não altera o pedido.
- RN04: O serviço de pagamentos guarda só os 4 últimos dígitos do cartão e o titular; nunca o número completo nem o CVV.
- RN05: Cada resultado gera um único e-mail ao cliente.
- RN06: Só o dono do pedido acompanha o resultado na tela.

## Limitações conhecidas

- O serviço de pagamentos é simulado: recusa cartões terminados em `0000` e aprova os demais.
- Até a solicitação de cobrança ser entregue ao serviço de pagamentos, os dados completos do cartão, inclusive o CVV, ficam guardados temporariamente no serviço de pedidos.

## Pós-condição

Pedido **Confirmado** ou com **Falha no pagamento**, e o cliente avisado por e-mail.
