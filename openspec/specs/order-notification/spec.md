# order-notification Specification

## Purpose

Define como o cliente é avisado por e-mail do resultado do pagamento do seu pedido, independentemente de continuar na loja.

## Requirements

### Requirement: E-mail com o resultado do pagamento
Quando o resultado do pagamento atualiza um pedido, a API de Orders DEVE enviar ao e-mail informado no pedido uma mensagem com o número do pedido, o total e o resultado: pedido confirmado (pagamento aprovado), pagamento recusado ou pagamento não processado (nada foi cobrado e o cliente pode refazer a compra).

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido A123, de total 25.00, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pagamento do pedido A123 é aprovado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pedido A123, de 25.00, foi confirmado

#### Scenario: Pagamento recusado
- **GIVEN** o pedido A123, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pagamento do pedido A123 é recusado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pagamento do pedido A123 foi recusado

#### Scenario: Pagamento não processado
- **GIVEN** o pedido A123, registrado com o e-mail cliente@exemplo.com, está pendente
- **WHEN** o pedido A123 fica com pagamento não processado
- **THEN** cliente@exemplo.com recebe um e-mail informando que o pagamento de A123 não foi processado e nada foi cobrado

#### Scenario: Cliente saiu da loja
- **GIVEN** o cliente fechou a loja logo depois de registrar o pedido A123
- **WHEN** o pagamento do pedido A123 é processado
- **THEN** o cliente recebe o e-mail com o resultado do pedido A123

### Requirement: Um e-mail por resultado
O cliente DEVE receber o e-mail do resultado de cada tentativa de pagamento mesmo que os primeiros envios falhem temporariamente, até o limite de tentativas do envio, e NÃO DEVE receber um segundo e-mail quando o mesmo resultado chega de novo. Uma falha no envio NÃO DEVE desfazer nem atrasar a atualização do status do pedido.

#### Scenario: Servidor de e-mail indisponível
- **GIVEN** o servidor de e-mail está fora do ar
- **WHEN** o pagamento do pedido A123 é aprovado
- **THEN** o pedido A123 fica confirmado imediatamente
- **AND** o cliente recebe o e-mail de confirmação quando o servidor de e-mail voltar

#### Scenario: Resultado repetido
- **GIVEN** o cliente já recebeu o e-mail de confirmação do pedido A123
- **WHEN** o resultado "aprovado" do pedido A123 chega de novo
- **THEN** nenhum novo e-mail é enviado

#### Scenario: Recusa e aprovação na nova tentativa
- **GIVEN** o cliente recebeu o e-mail da recusa da tentativa 1 do pedido A123
- **WHEN** a tentativa 2 de A123 é aprovada
- **THEN** o cliente recebe o e-mail de confirmação de A123

### Requirement: Limite de tentativas do e-mail
A API de Orders DEVE espaçar as tentativas de envio do e-mail de um pedido com intervalo crescente e DEVE encerrar o envio como falha definitiva depois de um número máximo de tentativas, ou na primeira recusa permanente do destinatário. Um pedido em falha definitiva NÃO DEVE ser tentado de novo.

#### Scenario: Destinatário recusado
- **GIVEN** o servidor de e-mail recusa o endereço do pedido A123 como inexistente
- **WHEN** o e-mail de A123 é enviado
- **THEN** o envio de A123 fica em falha definitiva, sem novas tentativas, e a falha é registrada com o número A123

#### Scenario: Erro temporário persistente
- **GIVEN** o envio do e-mail de A123 falha temporariamente em todas as tentativas
- **WHEN** o limite de tentativas é atingido
- **THEN** o envio de A123 fica em falha definitiva, e o intervalo entre as tentativas foi crescente

#### Scenario: Erro temporário que se resolve
- **GIVEN** o envio do e-mail de A123 falhou temporariamente uma vez
- **WHEN** a tentativa seguinte é feita depois do intervalo e o envio funciona
- **THEN** o cliente recebe o e-mail de A123

### Requirement: Servidor de e-mail indisponível não consome tentativas
Quando a falha é do servidor de e-mail (fora do ar, autenticação ou conexão segura), a API de Orders NÃO DEVE contar tentativas para os pedidos e DEVE retomar o envio, com intervalo crescente entre as verificações, quando o servidor voltar.

#### Scenario: Senha do SMTP errada por horas
- **GIVEN** a autenticação no servidor de e-mail falha por 3 horas e há 10 pedidos aguardando e-mail
- **WHEN** a autenticação é corrigida
- **THEN** os 10 clientes recebem o e-mail, sem nenhum pedido em falha definitiva

### Requirement: Um pedido não bloqueia os demais
Uma falha no e-mail de um pedido NÃO DEVE impedir o envio dos e-mails dos outros pedidos.

#### Scenario: Endereço inválido na frente da fila
- **GIVEN** o pedido A123 tem um endereço recusado e B456 tem um endereço válido
- **WHEN** a rodada de envio processa os dois
- **THEN** B456 recebe o e-mail, e A123 fica em falha definitiva

### Requirement: Saúde do envio de e-mail
A API de Orders DEVE expor uma verificação do envio de e-mail, separada da prontidão, que indique falha enquanto o servidor de e-mail estiver indisponível e alerta enquanto houver pedidos em falha definitiva nas últimas 24 horas. A prontidão da API NÃO DEVE depender do envio de e-mail.

#### Scenario: Servidor de e-mail fora
- **GIVEN** a última tentativa de envio falhou por indisponibilidade do servidor de e-mail
- **WHEN** a verificação de e-mail e a prontidão são consultadas
- **THEN** a verificação de e-mail responde HTTP 503 e a prontidão responde HTTP 200

#### Scenario: Pedido em falha definitiva
- **GIVEN** o servidor de e-mail está no ar e o pedido A123 entrou em falha definitiva há 1 hora
- **WHEN** a verificação de e-mail é consultada
- **THEN** ela indica alerta e informa a quantidade de pedidos em falha definitiva
