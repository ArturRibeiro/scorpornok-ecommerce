# Spec Delta

## Purpose

Define como o contexto Payments processa o pagamento de um pedido: recebe a solicitação, decide aprovar ou recusar, registra o pagamento sem dados sensíveis do cartão e comunica o resultado ao contexto de pedidos.

## ADDED Requirements

### Requirement: Processamento da solicitação de pagamento
O Payments DEVE processar cada solicitação de pagamento recebida do contexto de pedidos, cobrando o valor informado na solicitação, nas parcelas informadas.

#### Scenario: Solicitação recebida
- **GIVEN** uma solicitação de pagamento do pedido A123 no valor de 25.00 em 1 parcela
- **WHEN** o Payments recebe a solicitação
- **THEN** o pagamento do pedido A123 é processado no valor de 25.00 em 1 parcela

### Requirement: Decisão do pagamento
O Payments DEVE recusar o pagamento quando o número do cartão termina em `0000` e DEVE aprová-lo nos demais casos.

#### Scenario: Cartão aprovado
- **GIVEN** uma solicitação com o cartão 4111111111111111
- **WHEN** o Payments processa a solicitação
- **THEN** o pagamento é aprovado

#### Scenario: Cartão recusado
- **GIVEN** uma solicitação com o cartão 4111111111110000
- **WHEN** o Payments processa a solicitação
- **THEN** o pagamento é recusado com um motivo

### Requirement: Registro do pagamento
O Payments DEVE registrar cada pagamento processado com identificador próprio, número do pedido, valor, parcelas, titular, os quatro últimos dígitos do cartão, resultado e data. Ele NÃO DEVE guardar o CVV nem o número completo do cartão.

#### Scenario: Pagamento registrado sem dados sensíveis
- **GIVEN** uma solicitação com o cartão 4111111111111111 e CVV 123
- **WHEN** o pagamento é processado
- **THEN** o registro guarda os dígitos finais 1111 e o resultado
- **AND** nem o CVV nem o número completo aparecem no registro

### Requirement: Comunicação do resultado
Depois de registrar o pagamento, o Payments DEVE comunicar ao contexto de pedidos o resultado: aprovado com o identificador do pagamento, ou recusado com o motivo, sempre com o número do pedido.

#### Scenario: Resultado aprovado
- **GIVEN** o pagamento do pedido A123 foi aprovado
- **WHEN** o Payments termina o processamento
- **THEN** o contexto de pedidos recebe "aprovado" para A123 com o identificador do pagamento

#### Scenario: Resultado recusado
- **GIVEN** o pagamento do pedido A123 foi recusado
- **WHEN** o Payments termina o processamento
- **THEN** o contexto de pedidos recebe "recusado" para A123 com o motivo

### Requirement: Solicitação repetida
Uma solicitação repetida para um pedido que já tem pagamento registrado NÃO DEVE gerar uma nova cobrança. O Payments DEVE comunicar de novo o resultado já registrado.

#### Scenario: Mesma solicitação entregue duas vezes
- **GIVEN** o pagamento do pedido A123 já foi aprovado e registrado
- **WHEN** a mesma solicitação chega de novo
- **THEN** nenhum novo pagamento é registrado
- **AND** o contexto de pedidos recebe outra vez "aprovado" com o mesmo identificador de pagamento

### Requirement: Independência da disponibilidade
Solicitações feitas enquanto o Payments está fora do ar NÃO DEVEM ser perdidas. O Payments DEVE processá-las quando voltar.

#### Scenario: Payments volta a funcionar
- **GIVEN** o Payments estava parado quando o pedido A123 foi registrado
- **WHEN** o Payments volta a funcionar
- **THEN** o pagamento do pedido A123 é processado e o resultado é comunicado
