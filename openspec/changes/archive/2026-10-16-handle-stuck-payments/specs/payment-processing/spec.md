# Spec Delta

## ADDED Requirements

### Requirement: Consulta da situação do pagamento
O Payments DEVE responder à consulta sobre o pagamento de um pedido com uma de quatro situações: aprovado (com o identificador), recusado (com o motivo), em processamento ou sem pagamento. A consulta NÃO DEVE gerar cobrança.

#### Scenario: Pagamento já registrado
- **GIVEN** o pagamento do pedido A123 foi aprovado com o identificador P
- **WHEN** o Payments recebe a consulta de A123
- **THEN** ele responde "aprovado" com P, sem nova cobrança

#### Scenario: Pagamento em andamento
- **GIVEN** a cobrança do pedido A123 começou e ainda não terminou
- **WHEN** o Payments recebe a consulta de A123
- **THEN** ele responde "em processamento"

### Requirement: Pedido sem pagamento anulado
Ao responder "sem pagamento", o Payments DEVE anular o pedido; uma solicitação de pagamento que chegue depois para esse pedido NÃO DEVE ser cobrada e DEVE ser respondida como recusada.

#### Scenario: Solicitação atrasada
- **GIVEN** o Payments respondeu "sem pagamento" para A123
- **WHEN** a solicitação de pagamento de A123 chega depois
- **THEN** nada é cobrado e o resultado de A123 é "recusado"
