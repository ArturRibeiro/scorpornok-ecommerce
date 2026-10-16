# Spec Delta

## ADDED Requirements

### Requirement: Verificação do pedido pendente
A API de Orders DEVE consultar o Payments sobre o pagamento de um pedido que continua pendente além do prazo configurado, e repetir a consulta a cada intervalo enquanto não houver resposta definitiva. Ela NÃO DEVE mudar o status do pedido sem a resposta do Payments.

#### Scenario: Prazo esgotado
- **GIVEN** o prazo é de 15 minutos e o pedido A123 está pendente há 16 minutos
- **WHEN** a verificação roda
- **THEN** o Payments é consultado sobre o pagamento de A123

#### Scenario: Payments sem resposta
- **GIVEN** o pedido A123 passou do prazo e o Payments está parado
- **WHEN** a verificação roda
- **THEN** o pedido A123 continua pendente e é consultado de novo no intervalo seguinte

### Requirement: Desfecho da verificação
A API de Orders DEVE aplicar ao pedido pendente a resposta da consulta: aprovado → confirmado; recusado → falha no pagamento; sem pagamento → pagamento não processado; em processamento → continua pendente.

#### Scenario: Pagamento aprovado e resultado perdido
- **GIVEN** o pagamento de A123 foi aprovado, mas o resultado não chegou ao Orders
- **WHEN** a consulta de A123 responde "aprovado"
- **THEN** o pedido A123 fica confirmado com o identificador do pagamento

#### Scenario: Solicitação nunca processada
- **GIVEN** a solicitação de pagamento de A123 nunca foi processada
- **WHEN** a consulta de A123 responde "sem pagamento"
- **THEN** o pedido A123 fica com pagamento não processado

#### Scenario: Pagamento em andamento
- **GIVEN** o pagamento de A123 está em processamento
- **WHEN** a consulta de A123 responde "em processamento"
- **THEN** o pedido A123 continua pendente

### Requirement: Pagamento não processado avisado à loja
Quem acompanha um pedido com pagamento não processado DEVE receber esse desfecho, distinto de "pagamento recusado".

#### Scenario: Acompanhamento de pedido não processado
- **GIVEN** o pedido A123 está com pagamento não processado
- **WHEN** o dono começa a acompanhar A123
- **THEN** a loja recebe na hora A123 com pagamento não processado, e não como recusado

## MODIFIED Requirements

### Requirement: Resultado do pagamento no pedido
Ao receber o resultado do pagamento, a API de Orders DEVE atualizar o pedido pendente: aprovado → status confirmado e o identificador do pagamento guardado; recusado → status de falha no pagamento. Um resultado para um pedido que já não está pendente NÃO DEVE alterá-lo.

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido A123 está pendente
- **WHEN** chega o resultado "aprovado" com o identificador de pagamento P
- **THEN** o pedido A123 fica confirmado com o identificador de pagamento P

#### Scenario: Pagamento recusado
- **GIVEN** o pedido A123 está pendente
- **WHEN** chega o resultado "recusado"
- **THEN** o pedido A123 fica com falha no pagamento e sem identificador de pagamento

#### Scenario: Resultado repetido
- **GIVEN** o pedido A123 já está confirmado
- **WHEN** chega de novo um resultado para A123
- **THEN** o pedido continua confirmado, sem mudanças

#### Scenario: Resultado depois de não processado
- **GIVEN** o pedido A123 está com pagamento não processado
- **WHEN** chega um resultado "recusado" para A123
- **THEN** o pedido continua com pagamento não processado
