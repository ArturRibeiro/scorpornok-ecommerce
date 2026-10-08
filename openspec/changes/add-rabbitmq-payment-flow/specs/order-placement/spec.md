# Spec Delta

## ADDED Requirements

### Requirement: Solicitação de pagamento
Depois de persistir um pedido válido, a API de Orders DEVE solicitar o pagamento ao contexto Payments com o número do pedido, os dados do cartão recebidos e o total calculado pelo próprio pedido, ignorando qualquer valor de cobrança enviado pelo cliente. Um pedido inválido NÃO DEVE gerar solicitação.

#### Scenario: Pedido válido solicita pagamento
- **GIVEN** um pedido válido com 2 unidades a $10.00 e 1 unidade a $5.00, enviado com valor de cobrança 1.00 no cartão
- **WHEN** o pedido é registrado
- **THEN** o pagamento do pedido é solicitado no valor de 25.00

#### Scenario: Pedido inválido não solicita pagamento
- **GIVEN** um pedido sem itens
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 e nenhum pagamento é solicitado

#### Scenario: Payments fora do ar
- **GIVEN** o contexto Payments está parado
- **WHEN** um pedido válido é enviado
- **THEN** a resposta é HTTP 201 com o status pendente, como de costume

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

### Requirement: Notificação do resultado à loja
Depois de atualizar o pedido com o resultado do pagamento, a API de Orders DEVE enviar à loja que acompanha aquele pedido uma mensagem com o número do pedido e a indicação de aprovado ou recusado. Quem começar a acompanhar um pedido que já não está pendente DEVE receber o resultado imediatamente. Só as origens configuradas da loja DEVEM poder acompanhar pedidos.

#### Scenario: Resultado enviado a quem acompanha
- **GIVEN** a loja acompanha o pedido A123, que está pendente
- **WHEN** o pagamento do pedido A123 é aprovado e o pedido é confirmado
- **THEN** a loja recebe uma mensagem com o número A123 indicando pagamento aprovado

#### Scenario: Acompanhamento iniciado depois do resultado
- **GIVEN** o pedido A123 já está com falha no pagamento
- **WHEN** a loja começa a acompanhar o pedido A123
- **THEN** a loja recebe imediatamente uma mensagem com o número A123 indicando pagamento recusado

#### Scenario: Origem não configurada
- **GIVEN** a origem `http://exemplo.com` não está configurada
- **WHEN** o navegador tenta acompanhar um pedido a partir dela
- **THEN** a conexão é recusada
