# payment-processing Specification

## Purpose

Define como o contexto Payments processa o pagamento de um pedido: recebe a solicitação, decide aprovar ou recusar, registra o pagamento sem dados sensíveis do cartão e comunica o resultado ao contexto de pedidos.

## Requirements

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
O Payments DEVE registrar o pagamento como "em processamento" antes de cobrar e, com a resposta do gateway, completá-lo com o resultado. O registro tem identificador próprio, número do pedido, tentativa, valor, parcelas, titular, os quatro últimos dígitos do cartão, situação, motivo da recusa e data. Ele NÃO DEVE guardar o CVV nem o número completo do cartão, nem no pagamento nem no token.

#### Scenario: Pagamento registrado sem dados sensíveis
- **GIVEN** uma solicitação com o token do cartão 4111111111111111, tokenizado com CVV 123
- **WHEN** o pagamento é processado
- **THEN** o registro guarda os dígitos finais 1111 e o resultado
- **AND** nem o CVV nem o número completo aparecem no registro nem no token

#### Scenario: Pagamento em processamento
- **GIVEN** uma solicitação do pedido A123
- **WHEN** o gateway ainda não respondeu
- **THEN** existe um pagamento de A123 na situação "em processamento", sem resultado comunicado

### Requirement: Comunicação do resultado
Depois de registrar o pagamento, o Payments DEVE comunicar ao contexto de pedidos o resultado: aprovado com o identificador do pagamento, ou recusado com o motivo, sempre com o número do pedido.

#### Scenario: Resultado aprovado
- **GIVEN** o pagamento do pedido A123 foi aprovado
- **WHEN** o Payments termina o processamento
- **THEN** o contexto de pedidos recebe "aprovado" para A123 com o identificador do pagamento

#### Scenario: Resultado recusado
- **GIVEN** o pagamento do pedido A123 foi recusado
- **WHEN** o Payments termina o processamento
- **THEN** o contexto de pedidos recebe "recusado" para A123 com o motivo

### Requirement: Solicitação repetida
Uma solicitação repetida para uma tentativa que já tem pagamento registrado NÃO DEVE gerar uma nova cobrança. Se o pagamento tiver resultado, o Payments DEVE comunicá-lo de novo; se estiver em processamento, DEVE obter do gateway o resultado da cobrança original e então comunicá-lo.

#### Scenario: Mesma solicitação entregue duas vezes
- **GIVEN** o pagamento do pedido A123 já foi aprovado e registrado
- **WHEN** a mesma solicitação chega de novo
- **THEN** nenhum novo pagamento é registrado
- **AND** o contexto de pedidos recebe outra vez "aprovado" com o mesmo identificador de pagamento

#### Scenario: Falha depois da cobrança, antes de registrar o resultado
- **GIVEN** o gateway devolveu a aprovação da cobrança do pedido A123, mas o registro do pagamento ficou "em processamento" porque o Payments falhou antes de gravar o resultado
- **WHEN** a mesma solicitação de pagamento chega de novo
- **THEN** o gateway devolve a aprovação original, sem cobrar o cartão outra vez
- **AND** o pagamento fica registrado como aprovado e o contexto de pedidos recebe "aprovado" para A123

### Requirement: Independência da disponibilidade
Solicitações feitas enquanto o Payments está fora do ar NÃO DEVEM ser perdidas. O Payments DEVE processá-las quando voltar.

#### Scenario: Payments volta a funcionar
- **GIVEN** o Payments estava parado quando o pedido A123 foi registrado
- **WHEN** o Payments volta a funcionar
- **THEN** o pagamento do pedido A123 é processado e o resultado é comunicado

### Requirement: Cobrança única por tentativa
O Payments DEVE identificar cada cobrança ao gateway com uma chave formada pelo número do pedido e pela tentativa de pagamento. A mesma chave NÃO DEVE gerar mais de uma cobrança, mesmo que a solicitação seja processada mais de uma vez ou ao mesmo tempo.

#### Scenario: Falha ao registrar depois de cobrar
- **GIVEN** o cartão do pedido A123 foi cobrado, mas o registro do resultado falhou
- **WHEN** a mesma solicitação é entregue de novo
- **THEN** o cartão não é cobrado outra vez
- **AND** o pagamento é registrado com o resultado da primeira cobrança

#### Scenario: Entregas simultâneas
- **GIVEN** a solicitação do pedido A123 é entregue duas vezes ao mesmo tempo
- **WHEN** as duas entregas são processadas
- **THEN** o cartão é cobrado uma única vez e existe um único pagamento para A123

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
