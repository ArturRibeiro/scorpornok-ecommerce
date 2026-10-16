# Spec Delta

## MODIFIED Requirements

### Requirement: E-mail com o resultado do pagamento
Quando o resultado do pagamento atualiza um pedido, a API de Orders DEVE enviar ao e-mail informado no pedido uma mensagem com o número do pedido, o total e o resultado: pedido confirmado (pagamento aprovado), pagamento recusado ou pagamento não processado (nada foi cobrado e o cliente pode refazer a compra).

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido A123, de total 25.00, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pagamento do pedido A123 é aprovado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pedido A123, de 25.00, foi confirmado

#### Scenario: Pagamento recusado
- **GIVEN** o pedido A123, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pagamento do pedido A123 é recusado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pagamento do pedido A123 foi recusado

#### Scenario: Pagamento não processado
- **GIVEN** o pedido A123, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pedido A123 fica com pagamento não processado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pagamento de A123 não foi processado e nada foi cobrado

#### Scenario: Cliente saiu da loja
- **GIVEN** o cliente fechou a loja logo depois de registrar o pedido A123
- **WHEN** o pagamento do pedido A123 é processado
- **THEN** o cliente recebe o e-mail com o resultado do pedido A123
