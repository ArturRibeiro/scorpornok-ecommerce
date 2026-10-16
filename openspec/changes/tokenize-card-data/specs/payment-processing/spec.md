# Spec Delta

## ADDED Requirements

### Requirement: Tokenização do cartão
O Payments DEVE receber do navegador, a partir das origens configuradas da loja, o titular, o número, a validade e o CVV de um cartão e devolver um token opaco com os quatro últimos dígitos. Ele NÃO DEVE gravar o CVV nem o número completo, e DEVE recusar com HTTP 400 um cartão com dados inválidos.

#### Scenario: Cartão tokenizado
- **GIVEN** o cartão 4111111111111111, validade 12/2030 e CVV 123
- **WHEN** a loja pede a tokenização
- **THEN** a resposta traz um token e os dígitos finais 1111
- **AND** nem o CVV nem o número completo ficam gravados

#### Scenario: Cartão inválido
- **GIVEN** um cartão com CVV de 2 dígitos
- **WHEN** a loja pede a tokenização
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o CVV, e nenhum token é criado

### Requirement: Token de uso único e com validade
Um token DEVE valer para uma única cobrança e por 24 horas. Uma solicitação com token vencido, já usado ou desconhecido DEVE ser recusada com um motivo que diga isso, sem cobrança.

#### Scenario: Token vencido
- **GIVEN** o token T foi criado há mais de 24 horas e nunca foi usado
- **WHEN** chega a solicitação de pagamento do pedido A123 com o token T
- **THEN** o pagamento é recusado com o motivo de token inválido ou vencido, e nada é cobrado

#### Scenario: Token já usado
- **GIVEN** o token T já foi usado no pagamento do pedido A123
- **WHEN** chega a solicitação de pagamento do pedido B456 com o token T
- **THEN** o pagamento de B456 é recusado com o motivo de token inválido ou vencido

## MODIFIED Requirements

### Requirement: Processamento da solicitação de pagamento
O Payments DEVE processar cada solicitação de pagamento recebida do contexto de pedidos, cobrando pelo token do cartão o valor informado na solicitação, nas parcelas informadas.

#### Scenario: Solicitação recebida
- **GIVEN** uma solicitação de pagamento do pedido A123 com um token válido, no valor de 25.00 em 1 parcela
- **WHEN** o Payments recebe a solicitação
- **THEN** o pagamento do pedido A123 é processado no valor de 25.00 em 1 parcela

### Requirement: Decisão do pagamento
O Payments DEVE recusar o pagamento quando o cartão do token termina em `0000` e DEVE aprová-lo nos demais casos.

#### Scenario: Cartão aprovado
- **GIVEN** uma solicitação com o token do cartão 4111111111111111
- **WHEN** o Payments processa a solicitação
- **THEN** o pagamento é aprovado

#### Scenario: Cartão recusado
- **GIVEN** uma solicitação com o token do cartão 4111111111110000
- **WHEN** o Payments processa a solicitação
- **THEN** o pagamento é recusado com um motivo

### Requirement: Registro do pagamento
O Payments DEVE registrar cada pagamento processado com identificador próprio, número do pedido, valor, parcelas, titular, os quatro últimos dígitos do cartão, resultado e data. Ele NÃO DEVE guardar o CVV nem o número completo do cartão, nem no pagamento nem no token.

#### Scenario: Pagamento registrado sem dados sensíveis
- **GIVEN** uma solicitação com o token do cartão 4111111111111111, tokenizado com CVV 123
- **WHEN** o pagamento é processado
- **THEN** o registro guarda os dígitos finais 1111 e o resultado
- **AND** nem o CVV nem o número completo aparecem no registro nem no token
