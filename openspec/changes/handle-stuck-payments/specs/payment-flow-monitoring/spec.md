# Spec Delta

## Purpose

Visibilidade operacional do fluxo de pagamento: mensagens paradas nas filas de erro, pedidos presos além do prazo e o procedimento para reprocessá-los.

## ADDED Requirements

### Requirement: Filas de erro visíveis
O Orders e o Payments DEVEM expor uma verificação de saúde operacional que falha enquanto houver mensagens nas filas de erro que consomem. Essa verificação NÃO DEVE afetar a prontidão da API.

#### Scenario: Mensagem na fila de erro
- **GIVEN** um resultado de pagamento esgotou as tentativas e foi para a fila de erro do Orders
- **WHEN** a verificação operacional do Orders é consultada
- **THEN** ela indica falha e cita a fila e a quantidade de mensagens
- **AND** a prontidão do Orders continua indicando que ele está pronto

#### Scenario: Filas vazias
- **GIVEN** nenhuma fila de erro tem mensagens
- **WHEN** a verificação operacional é consultada
- **THEN** ela indica sucesso

### Requirement: Pedidos presos visíveis
A verificação operacional do Orders DEVE falhar enquanto houver pedido pendente além do prazo de verificação somado a uma tolerância configurável.

#### Scenario: Pedido preso
- **GIVEN** o pedido A123 está pendente além do prazo e da tolerância, porque o Payments não responde
- **WHEN** a verificação operacional do Orders é consultada
- **THEN** ela indica falha e cita quantos pedidos estão presos

### Requirement: Reprocessamento seguro
Uma mensagem devolvida da fila de erro para a fila de origem DEVE ser processada sem cobrança em dobro e sem alterar um pedido que já tem desfecho.

#### Scenario: Solicitação reprocessada
- **GIVEN** a solicitação de pagamento de A123 está na fila de erro e o pagamento já foi registrado
- **WHEN** a operação a devolve para a fila de origem
- **THEN** nada é cobrado de novo e o resultado registrado é comunicado outra vez
