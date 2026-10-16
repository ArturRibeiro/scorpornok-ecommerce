# Spec Delta

## ADDED Requirements

### Requirement: Nova tentativa de pagamento
A API de Orders DEVE aceitar do dono de um pedido com falha no pagamento um novo cartão, voltar o pedido a pendente e solicitar de novo o pagamento do mesmo total. Cada pedido DEVE ter no máximo 3 tentativas.

#### Scenario: Nova tentativa aceita
- **GIVEN** o pedido A123, do cliente C, está com falha no pagamento após 1 tentativa
- **WHEN** C envia outro cartão para A123
- **THEN** a resposta é HTTP 202, A123 volta a pendente e o pagamento é solicitado como tentativa 2

#### Scenario: Pedido não recusado
- **GIVEN** o pedido A123 está pendente ou confirmado
- **WHEN** o dono envia outro cartão para A123
- **THEN** a resposta é HTTP 409, e nada muda nem é cobrado

#### Scenario: Limite de tentativas
- **GIVEN** o pedido A123 já teve 3 tentativas recusadas
- **WHEN** o dono envia outro cartão para A123
- **THEN** a resposta é HTTP 409, e nada é cobrado

#### Scenario: Pedido de outro cliente
- **GIVEN** o pedido A123 pertence ao cliente C
- **WHEN** o cliente D envia um cartão para A123
- **THEN** a resposta é HTTP 404, e nada muda

## MODIFIED Requirements

### Requirement: Resultado do pagamento no pedido
Ao receber o resultado do pagamento, a API de Orders DEVE atualizar o pedido pendente: aprovado → status confirmado e o identificador do pagamento guardado; recusado → status de falha no pagamento. Um resultado de outra tentativa que não a atual, ou para um pedido que já não está pendente, NÃO DEVE alterá-lo.

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

#### Scenario: Resultado atrasado de tentativa anterior
- **GIVEN** o pedido A123 está pendente na tentativa 2
- **WHEN** chega de novo o resultado "recusado" da tentativa 1
- **THEN** o pedido continua pendente na tentativa 2
