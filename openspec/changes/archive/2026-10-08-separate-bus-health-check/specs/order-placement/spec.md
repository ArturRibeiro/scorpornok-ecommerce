# Spec Delta

## ADDED Requirements

### Requirement: Prontidão independente do broker
A verificação de prontidão da API de Orders DEVE indicar que ela está pronta sempre que o seu banco de dados estiver disponível, mesmo com o broker de mensagens indisponível, porque o registro de pedidos não depende do broker. A API DEVE expor a disponibilidade do broker numa verificação separada, que indica falha enquanto o broker estiver indisponível.

#### Scenario: Broker indisponível
- **GIVEN** o banco do Orders está no ar e o broker de mensagens está parado
- **WHEN** a prontidão da API de Orders é consultada
- **THEN** a resposta indica que a API está pronta (HTTP 200)
- **AND** a verificação do broker responde com falha (HTTP 503)

#### Scenario: Tudo no ar
- **GIVEN** o banco do Orders e o broker de mensagens estão no ar
- **WHEN** a prontidão e a verificação do broker são consultadas
- **THEN** as duas respondem HTTP 200

#### Scenario: Banco indisponível
- **GIVEN** o banco do Orders está parado
- **WHEN** a prontidão da API de Orders é consultada
- **THEN** a resposta indica que a API não está pronta (HTTP 503)
