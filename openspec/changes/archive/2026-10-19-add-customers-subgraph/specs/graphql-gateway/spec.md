# Spec Delta

## ADDED Requirements

### Requirement: Cliente identificado no ponto de entrada
O ponto de entrada DEVE identificar o cliente pela credencial da sessão e repassá-la às APIs. Credencial inválida ou expirada DEVE ser recusada; consulta que exige login sem credencial, ou de cliente sem e-mail confirmado, DEVE receber erro de acesso, sem dados.

#### Scenario: Sem credencial
- **GIVEN** não há cliente identificado
- **WHEN** alguém consulta o cadastro pelo ponto de entrada
- **THEN** a resposta traz erro de acesso não autenticado e nenhum dado do cadastro

#### Scenario: E-mail não confirmado
- **GIVEN** o cliente está autenticado com o e-mail não confirmado
- **WHEN** ele consulta o cadastro
- **THEN** a resposta traz erro de acesso não autorizado

#### Scenario: Credencial expirada
- **GIVEN** a credencial da sessão expirou
- **WHEN** a loja chama o ponto de entrada com ela
- **THEN** a chamada é recusada como não autenticada e a loja pede o login de novo

#### Scenario: Catálogo sem login
- **GIVEN** não há cliente identificado
- **WHEN** a loja consulta os produtos
- **THEN** os produtos são devolvidos normalmente

### Requirement: Cadastro pelo ponto de entrada
A loja DEVE ler e salvar o cadastro do cliente pelo ponto de entrada único, com as mesmas regras e mensagens de hoje. Os erros de validação DEVEM vir por campo, sem nada salvo.

#### Scenario: Salvar com CPF inválido
- **GIVEN** o cliente está identificado
- **WHEN** salva o cadastro com o CPF 111.111.111-11
- **THEN** a resposta traz o erro no campo CPF e o cadastro não muda

#### Scenario: Meu cadastro pelo ponto de entrada
- **GIVEN** o cliente tem cadastro salvo
- **WHEN** abre "Meu cadastro" ou o checkout
- **THEN** vê os mesmos dados de antes, sem chamada direta à API de cadastro
