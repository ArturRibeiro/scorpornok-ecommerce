# order-notification Specification

## Purpose

Define como o cliente é avisado por e-mail do resultado do pagamento do seu pedido, independentemente de continuar na loja.

## Requirements

### Requirement: E-mail com o resultado do pagamento
Quando o resultado do pagamento atualiza um pedido, a API de Orders DEVE enviar ao e-mail informado no pedido uma mensagem com o número do pedido, o total e o resultado: pedido confirmado (pagamento aprovado) ou pagamento recusado.

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido A123, de total 25.00, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pagamento do pedido A123 é aprovado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pedido A123, de 25.00, foi confirmado

#### Scenario: Pagamento recusado
- **GIVEN** o pedido A123, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pagamento do pedido A123 é recusado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pagamento do pedido A123 foi recusado

#### Scenario: Cliente saiu da loja
- **GIVEN** o cliente fechou a loja logo depois de registrar o pedido A123
- **WHEN** o pagamento do pedido A123 é processado
- **THEN** o cliente recebe o e-mail com o resultado do pedido A123

### Requirement: Um e-mail por resultado
O cliente DEVE receber o e-mail do resultado mesmo que o primeiro envio falhe, e NÃO DEVE receber um segundo e-mail quando o mesmo resultado chega de novo. Uma falha no envio NÃO DEVE desfazer nem atrasar a atualização do status do pedido.

#### Scenario: Servidor de e-mail indisponível
- **GIVEN** o servidor de e-mail está fora do ar
- **WHEN** o pagamento do pedido A123 é aprovado
- **THEN** o pedido A123 fica confirmado imediatamente
- **AND** o cliente recebe o e-mail de confirmação quando o servidor de e-mail voltar

#### Scenario: Resultado repetido
- **GIVEN** o cliente já recebeu o e-mail de confirmação do pedido A123
- **WHEN** o resultado "aprovado" do pedido A123 chega de novo
- **THEN** nenhum novo e-mail é enviado
