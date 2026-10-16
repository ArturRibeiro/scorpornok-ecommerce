# Spec Delta

## ADDED Requirements

### Requirement: Cobrança única por tentativa
O Payments DEVE identificar cada cobrança ao gateway com uma chave formada pelo número do pedido e pela tentativa de pagamento. A mesma chave NÃO DEVE gerar mais de uma cobrança, mesmo que a solicitação seja processada mais de uma vez ou ao mesmo tempo.

#### Scenario: Falha ao registrar depois de cobrar
- **GIVEN** o cartão do pedido A123 foi cobrado, mas o registro do resultado falhou
- **WHEN** a mesma solicitação é entregue de novo
- **THEN** o cartão não é cobrado outra vez
- **AND** o pagamento é registrado com o resultado da primeira cobrança

#### Scenario: Entregas simultâneas
- **GIVEN** a solicitação do pedido A123 é entregue duas vezes ao mesmo tempo
- **WHEN** as duas entregas são processadas
- **THEN** o cartão é cobrado uma única vez e existe um único pagamento para A123

## MODIFIED Requirements

### Requirement: Registro do pagamento
O Payments DEVE registrar o pagamento como "em processamento" antes de cobrar e, com a resposta do gateway, completá-lo com o resultado. O registro tem identificador próprio, número do pedido, tentativa, valor, parcelas, titular, os quatro últimos dígitos do cartão, situação, motivo da recusa e data. Ele NÃO DEVE guardar o CVV nem o número completo do cartão.

#### Scenario: Pagamento registrado sem dados sensíveis
- **GIVEN** uma solicitação com o cartão 4111111111111111 e CVV 123
- **WHEN** o pagamento é processado
- **THEN** o registro guarda os dígitos finais 1111 e o resultado
- **AND** nem o CVV nem o número completo aparecem no registro

#### Scenario: Pagamento em processamento
- **GIVEN** uma solicitação do pedido A123
- **WHEN** o gateway ainda não respondeu
- **THEN** existe um pagamento de A123 na situação "em processamento", sem resultado comunicado

### Requirement: Solicitação repetida
Uma solicitação repetida para uma tentativa que já tem pagamento registrado NÃO DEVE gerar uma nova cobrança. Se o pagamento tiver resultado, o Payments DEVE comunicá-lo de novo; se estiver em processamento, DEVE obter do gateway o resultado da cobrança original e então comunicá-lo.

#### Scenario: Mesma solicitação entregue duas vezes
- **GIVEN** o pagamento do pedido A123 já foi aprovado e registrado
- **WHEN** a mesma solicitação chega de novo
- **THEN** nenhum novo pagamento é registrado
- **AND** o contexto de pedidos recebe outra vez "aprovado" com o mesmo identificador de pagamento

#### Scenario: Reentrega com pagamento em processamento
- **GIVEN** o pagamento do pedido A123 está em processamento e o gateway já aprovou a cobrança
- **WHEN** a mesma solicitação chega de novo
- **THEN** o pagamento é completado como aprovado, sem nova cobrança
- **AND** o contexto de pedidos recebe "aprovado" para A123
