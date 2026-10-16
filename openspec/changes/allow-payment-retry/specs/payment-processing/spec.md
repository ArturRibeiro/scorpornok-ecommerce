# Spec Delta

## MODIFIED Requirements

### Requirement: Solicitação repetida
Uma solicitação repetida para uma tentativa de pagamento que já tem pagamento registrado NÃO DEVE gerar uma nova cobrança. O Payments DEVE comunicar de novo o resultado já registrado. Uma nova tentativa do mesmo pedido DEVE ser processada como um novo pagamento.

#### Scenario: Mesma solicitação entregue duas vezes
- **GIVEN** o pagamento da tentativa 1 do pedido A123 já foi aprovado e registrado
- **WHEN** a mesma solicitação chega de novo
- **THEN** nenhum novo pagamento é registrado
- **AND** o contexto de pedidos recebe outra vez "aprovado" com o mesmo identificador de pagamento

#### Scenario: Nova tentativa do mesmo pedido
- **GIVEN** o pagamento da tentativa 1 do pedido A123 foi recusado
- **WHEN** chega a solicitação da tentativa 2 de A123
- **THEN** o pagamento é processado e registrado como tentativa 2
- **AND** o resultado é comunicado com o número da tentativa
