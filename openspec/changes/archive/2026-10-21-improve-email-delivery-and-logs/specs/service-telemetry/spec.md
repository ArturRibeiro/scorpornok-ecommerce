# Spec Delta

## ADDED Requirements

### Requirement: Envio de e-mail no trace
O trace do envio do e-mail do resultado DEVE mostrar cada etapa com o servidor de e-mail (conexão, autenticação e envio de cada mensagem) com a sua duração, e NÃO DEVE conter o endereço do destinatário.

#### Scenario: Rodada com dois e-mails
- **GIVEN** dois pedidos aguardam o e-mail do resultado
- **WHEN** a rodada de envio roda
- **THEN** o trace mostra uma conexão, uma autenticação e dois envios, cada envio com o número do seu pedido

#### Scenario: Servidor fora do ar
- **GIVEN** o servidor de e-mail não aceita conexões
- **WHEN** a rodada de envio tenta conectar
- **THEN** o trace mostra a etapa de conexão com erro e o tipo da falha

#### Scenario: Destinatário fora do trace
- **WHEN** o e-mail do pedido de maria.souza@exemplo.com é enviado
- **THEN** o endereço não aparece em nenhum span

### Requirement: Logs sem repetir os spans
Os logs informativos que só repetem o que um span já registra (início, fim e duração de cada chamada HTTP) NÃO DEVEM ir ao coletor. Avisos e erros dessas mesmas fontes DEVEM ir.

#### Scenario: Chamada ao catálogo
- **GIVEN** o coletor de telemetria está no ar
- **WHEN** o serviço de pedidos consulta o catálogo durante uma compra
- **THEN** a chamada aparece só como span, sem os logs de início e fim

#### Scenario: Falha numa chamada HTTP
- **GIVEN** o catálogo está fora do ar
- **WHEN** o serviço de pedidos tenta consultá-lo
- **THEN** o aviso da falha é enviado ao coletor
