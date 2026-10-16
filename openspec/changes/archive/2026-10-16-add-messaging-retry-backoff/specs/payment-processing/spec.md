# Spec Delta

## ADDED Requirements

### Requirement: Falha transitória no processamento
Uma solicitação de pagamento cujo processamento falhe por indisponibilidade temporária DEVE ser processada sem nova ação do cliente, desde que a falha dure no máximo 5 minutos. Uma solicitação com dados inválidos NÃO DEVE ser tentada de novo.

#### Scenario: Banco do Payments volta em 2 minutos
- **GIVEN** o banco do Payments fica indisponível por 2 minutos
- **WHEN** chega a solicitação de pagamento do pedido A123
- **THEN** o pagamento de A123 é processado e o resultado é comunicado, sem passar pela fila de erro

#### Scenario: Falha além do limite
- **GIVEN** o banco do Payments fica indisponível por 10 minutos
- **WHEN** chega a solicitação de pagamento do pedido A123
- **THEN** a solicitação vai para a fila de erro depois de cerca de 5 minutos de tentativas

#### Scenario: Solicitação inválida
- **GIVEN** uma solicitação com dados inválidos
- **WHEN** o processamento a recusa
- **THEN** ela vai para a fila de erro na primeira falha, sem novas tentativas

#### Scenario: Payments reiniciado durante as tentativas
- **GIVEN** a solicitação do pedido A123 está em novas tentativas
- **WHEN** o Payments é reiniciado
- **THEN** a solicitação não se perde e é processada quando ele voltar
