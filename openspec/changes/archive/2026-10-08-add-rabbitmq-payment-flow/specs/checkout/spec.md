# Spec Delta

## MODIFIED Requirements

### Requirement: Pedido registrado
Quando o serviço de pedidos aceita o pedido, o checkout DEVE mostrar o número do pedido, DEVE indicar que o pagamento está em processamento e DEVE esvaziar o carrinho.

#### Scenario: Pedido aceito
- **GIVEN** o formulário está válido e o serviço de pedidos está no ar
- **WHEN** o cliente confirma o pedido e o serviço o aceita
- **THEN** é exibida uma confirmação com o número do pedido e a indicação de que o pagamento está em processamento
- **AND** o carrinho fica vazio, inclusive no contador do cabeçalho

## ADDED Requirements

### Requirement: Acompanhamento do pagamento
Depois que o pedido é aceito, o checkout DEVE receber do serviço de pedidos a mensagem com o resultado do pagamento, sem consultar o pedido periodicamente, e DEVE mostrar se o pagamento foi aprovado ou recusado. Se a mensagem não chegar em até 30 segundos, ou se a conexão para recebê-la não puder ser aberta, o checkout DEVE informar que o pagamento continua em processamento.

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido foi aceito e a confirmação indica pagamento em processamento
- **WHEN** chega a mensagem de que o pagamento do pedido foi aprovado
- **THEN** a confirmação mostra que o pagamento foi aprovado
- **AND** o checkout não fez consultas periódicas ao pedido

#### Scenario: Pagamento recusado
- **GIVEN** o pedido foi aceito e a confirmação indica pagamento em processamento
- **WHEN** chega a mensagem de que o pagamento do pedido foi recusado
- **THEN** a confirmação mostra que o pagamento foi recusado

#### Scenario: Mensagem não chega
- **GIVEN** o pedido foi aceito e o pagamento não é processado
- **WHEN** passam 30 segundos sem a mensagem do resultado
- **THEN** o checkout informa que o pagamento ainda está em processamento
