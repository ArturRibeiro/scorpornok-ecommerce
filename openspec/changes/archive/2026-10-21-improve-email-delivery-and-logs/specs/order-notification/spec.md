# Spec Delta

## ADDED Requirements

### Requirement: Uma conexão por rodada de envio
A API de Orders DEVE enviar os e-mails de uma rodada pela mesma conexão com o servidor de e-mail, aberta só quando há e-mail a enviar e encerrada ao fim da rodada.

#### Scenario: Rodada com vários pedidos
- **GIVEN** três pedidos aguardam o e-mail do resultado
- **WHEN** a rodada de envio roda
- **THEN** os três e-mails são enviados por uma única conexão com o servidor

#### Scenario: Rodada sem pedidos
- **GIVEN** nenhum pedido aguarda e-mail
- **WHEN** a rodada de envio roda
- **THEN** nenhuma conexão com o servidor de e-mail é aberta

#### Scenario: Destinatário recusado no meio da rodada
- **GIVEN** três pedidos aguardam e-mail e o servidor recusa o destinatário do segundo
- **WHEN** a rodada de envio roda
- **THEN** o segundo pedido fica em falha definitiva e o terceiro é enviado pela mesma conexão

#### Scenario: Conexão cai no meio da rodada
- **GIVEN** três pedidos aguardam e-mail e a conexão cai depois do primeiro envio
- **WHEN** a rodada continua
- **THEN** o segundo e o terceiro ficam para a próxima rodada, sem contar tentativa
