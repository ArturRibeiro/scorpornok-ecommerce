# Spec Delta

## ADDED Requirements

### Requirement: Falha transitória ao aplicar o resultado
Um resultado de pagamento cuja aplicação ao pedido falhe por indisponibilidade temporária DEVE ser aplicado sem intervenção, desde que a falha dure no máximo 5 minutos. A falha NÃO DEVE perder o resultado, mesmo que a API de Orders seja reiniciada durante as tentativas.

#### Scenario: Banco do Orders volta em 2 minutos
- **GIVEN** o pedido A123 está pendente e o banco do Orders fica indisponível por 2 minutos
- **WHEN** chega o resultado "aprovado" de A123
- **THEN** A123 fica confirmado e o cliente é avisado, sem o resultado passar pela fila de erro

#### Scenario: Falha além do limite
- **GIVEN** o banco do Orders fica indisponível por 10 minutos
- **WHEN** chega o resultado do pagamento de A123
- **THEN** o resultado vai para a fila de erro depois de cerca de 5 minutos de tentativas

#### Scenario: Orders reiniciado durante as tentativas
- **GIVEN** o resultado de A123 está em novas tentativas
- **WHEN** a API de Orders é reiniciada
- **THEN** o resultado não se perde e é aplicado quando ela voltar
