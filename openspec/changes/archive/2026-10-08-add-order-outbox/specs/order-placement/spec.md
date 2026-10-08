# Spec Delta

## ADDED Requirements

### Requirement: Registro do pedido independente do broker
A API de Orders DEVE responder ao registro de um pedido válido em até 5 segundos mesmo com o broker de mensagens indisponível, com o mesmo contrato de sempre (HTTP 201 e pedido pendente). A solicitação de pagamento DEVE ser entregue ao contexto Payments assim que o broker voltar, sem novo envio do cliente.

#### Scenario: Broker indisponível no registro
- **GIVEN** o broker de mensagens está parado
- **WHEN** um pedido válido é enviado
- **THEN** a resposta é HTTP 201 com o status pendente em até 5 segundos

#### Scenario: Broker volta depois do registro
- **GIVEN** o pedido A123 foi registrado com o broker parado e está pendente
- **WHEN** o broker volta a funcionar
- **THEN** o pagamento do pedido A123 é solicitado e o pedido deixa de estar pendente

#### Scenario: API reiniciada antes de o broker voltar
- **GIVEN** o pedido A123 foi registrado com o broker parado
- **WHEN** a API de Orders é reiniciada e só depois o broker volta
- **THEN** o pagamento do pedido A123 ainda é solicitado

### Requirement: Pedido e solicitação de pagamento atômicos
Um pedido válido registrado DEVE ter a sua solicitação de pagamento registrada junto, de forma atômica: ou os dois ficam registrados, ou nenhum. Nenhum pedido registrado DEVE ficar sem solicitação de pagamento por falha de comunicação com o broker.

#### Scenario: Falha ao registrar
- **GIVEN** o banco de dados do Orders falha durante o registro de um pedido válido
- **WHEN** o pedido é enviado
- **THEN** a resposta é de erro, o pedido não fica registrado e nenhum pagamento é solicitado

#### Scenario: Pedido sem cartão
- **GIVEN** um pedido válido enviado sem dados de cartão
- **WHEN** o pedido é registrado
- **THEN** a resposta é HTTP 201 com o status pendente e nenhum pagamento é solicitado
