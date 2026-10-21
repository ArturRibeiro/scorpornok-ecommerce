# Spec Delta

## Purpose

Telemetria dos serviços do back-end (traces distribuídos, métricas e logs correlacionados) para diagnosticar uma compra de ponta a ponta, sem expor dados sensíveis e sem tornar o coletor uma dependência das APIs.

## ADDED Requirements

### Requirement: Trace distribuído da compra
Cada serviço do back-end DEVE emitir spans das requisições que recebe, das chamadas HTTP que faz, das consultas ao banco e das mensagens que publica e consome. Um mesmo trace DEVE ligar a requisição de origem às chamadas entre serviços e às mensagens que ela causa, inclusive as que saem depois pelo outbox.

#### Scenario: Pedido com pagamento
- **GIVEN** o coletor de telemetria está no ar
- **WHEN** o cliente registra um pedido com cartão pelo ponto de entrada
- **THEN** um único trace contém o ponto de entrada, o serviço de pedidos, a consulta ao catálogo, a solicitação de pagamento, o processamento no serviço de pagamentos e a aplicação do resultado no serviço de pedidos

#### Scenario: Mensagem publicada com o broker fora
- **GIVEN** o broker está fora do ar quando o pedido é registrado
- **WHEN** o broker volta e a mensagem pendente é entregue
- **THEN** o processamento da mensagem aparece no mesmo trace do registro do pedido

### Requirement: Trace nomeado pela operação
O span de uma requisição GraphQL DEVE ser nomeado pelo tipo e pelo nome da operação, em cada serviço que a recebe, e não pelo caminho HTTP.

#### Scenario: Registro do pedido
- **GIVEN** o coletor de telemetria está no ar
- **WHEN** a loja envia a operação `mutation PlaceOrder` ao ponto de entrada
- **THEN** o trace aparece com o nome `mutation PlaceOrder`

#### Scenario: Operação sem nome
- **WHEN** a loja envia uma consulta sem nome de operação
- **THEN** o span da requisição é nomeado só pelo tipo, `query`

### Requirement: Trace localizável pelo pedido
Os spans que tratam de um pedido DEVEM trazer o número do pedido em todos os serviços, e o registro do pedido e o resultado do pagamento DEVEM gerar um log com ele.

#### Scenario: Busca pelo número
- **GIVEN** o pedido A518844992148 foi registrado e pago
- **WHEN** alguém filtra a telemetria pelo número A518844992148
- **THEN** encontra o trace da compra, com os spans do serviço de pedidos, do serviço de pagamentos e do cadastro, e os logs do registro e do resultado

#### Scenario: Rodada com vários pedidos
- **GIVEN** a verificação de pedidos pendentes encontra dois pedidos
- **WHEN** ela consulta o pagamento de cada um
- **THEN** cada consulta aparece num span com o número do seu pedido

### Requirement: Identificação do serviço
Toda telemetria DEVE identificar o serviço que a emitiu, o ambiente e a versão.

#### Scenario: Span do serviço de pagamentos
- **GIVEN** as APIs rodam no compose
- **WHEN** o serviço de pagamentos emite um span
- **THEN** o span traz o nome do serviço de pagamentos, o ambiente e a versão

### Requirement: Logs ligados ao trace
Os logs das APIs DEVEM ser enviados ao coletor com o identificador do trace e do span em que foram escritos, quando houver um.

#### Scenario: Log durante uma requisição
- **GIVEN** uma requisição está em andamento
- **WHEN** o serviço escreve um log
- **THEN** o log enviado traz o identificador do trace dessa requisição

### Requirement: Dados sensíveis fora da telemetria
Spans, métricas e logs enviados NÃO DEVEM conter número do cartão, validade, CVV, CPF, telefone, e-mail do cliente, token de acesso nem o texto ou as variáveis das operações GraphQL. A operação GraphQL DEVE ser identificada só pelo tipo e pelo nome.

#### Scenario: Tokenização do cartão
- **GIVEN** o coletor de telemetria está no ar
- **WHEN** a loja tokeniza o cartão 4111111111111111 com CVV 123 pelo ponto de entrada, com sucesso ou com o serviço de pagamentos fora
- **THEN** o número e o CVV não aparecem em nenhum span, métrica ou log enviado

#### Scenario: Acompanhamento do pagamento
- **GIVEN** o navegador acompanha um pedido com o token de acesso no endereço da conexão
- **WHEN** a conexão é registrada na telemetria
- **THEN** o token não aparece no span

#### Scenario: Pedido com dados do comprador
- **GIVEN** o coletor de telemetria está no ar
- **WHEN** o cliente registra um pedido com CPF, telefone e e-mail
- **THEN** esses valores não aparecem em nenhum span, métrica ou log enviado

### Requirement: Telemetria sem ruído
Health checks e rodadas dos serviços em segundo plano que não encontram trabalho NÃO DEVEM gerar traces. Rodadas que processam algo DEVEM gerar.

#### Scenario: Health check
- **WHEN** um health check é consultado
- **THEN** nenhum trace é enviado

#### Scenario: Rodada com pedido pendente
- **GIVEN** há um pedido pendente além do prazo
- **WHEN** a verificação de pedidos pendentes o encontra
- **THEN** a consulta do pagamento aparece num trace

### Requirement: Funcionamento sem coletor
As APIs DEVEM subir e atender normalmente sem coletor configurado ou com o coletor fora do ar, sem exportar nada no primeiro caso e sem falhar requisições no segundo.

#### Scenario: Sem coletor configurado
- **GIVEN** nenhum coletor está configurado
- **WHEN** a API sobe e recebe requisições
- **THEN** ela responde como hoje e não tenta enviar telemetria

#### Scenario: Coletor fora do ar
- **GIVEN** o coletor configurado está fora do ar
- **WHEN** o cliente registra um pedido
- **THEN** o pedido é registrado normalmente
