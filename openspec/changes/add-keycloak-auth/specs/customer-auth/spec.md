# Spec Delta

## Purpose

Conta do cliente na loja: cadastro com e-mail e senha, confirmação do e-mail, login e logout, para que só clientes identificados finalizem compras.

## ADDED Requirements

### Requirement: Cadastro com e-mail e senha
A loja DEVE permitir criar uma conta com e-mail e senha de no mínimo 8 caracteres. O e-mail é o identificador de login.

#### Scenario: Cadastro válido
- **GIVEN** não existe conta para cliente@exemplo.com
- **WHEN** o cliente se cadastra com cliente@exemplo.com e uma senha de 8 caracteres
- **THEN** a conta é criada com o e-mail não confirmado

#### Scenario: Senha curta
- **GIVEN** a senha tem 7 caracteres
- **WHEN** o cliente tenta se cadastrar
- **THEN** a conta não é criada e a regra da senha é exibida

### Requirement: E-mail único
A loja NÃO DEVE criar uma segunda conta para um e-mail já cadastrado, e DEVE oferecer o login.

#### Scenario: E-mail já cadastrado
- **GIVEN** já existe conta para cliente@exemplo.com
- **WHEN** alguém tenta se cadastrar com cliente@exemplo.com
- **THEN** nenhuma conta é criada e é oferecido o login

### Requirement: Confirmação do e-mail
Ao criar a conta, a loja DEVE enviar um link de confirmação ao e-mail cadastrado. Até o link ser aberto, o login DEVE ser bloqueado, com a opção de reenviar o link.

#### Scenario: Link enviado e aberto
- **GIVEN** o cliente acabou de se cadastrar com cliente@exemplo.com
- **WHEN** ele abre o link recebido em cliente@exemplo.com
- **THEN** o e-mail da conta fica confirmado

#### Scenario: Login antes de confirmar
- **GIVEN** a conta de cliente@exemplo.com não está confirmada
- **WHEN** o cliente entra com a senha correta e pede o reenvio
- **THEN** a sessão não é iniciada e cliente@exemplo.com recebe um novo link

### Requirement: Login com e-mail e senha
A loja DEVE iniciar a sessão com o e-mail e a senha corretos de uma conta confirmada. A mensagem de falha NÃO DEVE revelar se o e-mail existe.

#### Scenario: Credenciais corretas
- **GIVEN** existe conta confirmada para cliente@exemplo.com
- **WHEN** o cliente entra com a senha correta
- **THEN** a sessão é iniciada e a loja mostra o e-mail dele

#### Scenario: Credenciais incorretas
- **GIVEN** existe conta para cliente@exemplo.com
- **WHEN** o cliente entra com a senha errada
- **THEN** a sessão não é iniciada, com a mesma mensagem de um e-mail inexistente

### Requirement: Logout
A loja DEVE permitir encerrar a sessão; depois disso, o checkout DEVE exigir login de novo.

#### Scenario: Sair
- **GIVEN** o cliente está identificado
- **WHEN** ele sai e vai para o checkout
- **THEN** ele é levado ao login
