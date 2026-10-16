# customer-auth Specification

## Purpose

Conta do cliente na loja: cadastro pelo e-mail, confirmação do e-mail com definição da senha, login e logout, para que só clientes identificados finalizem compras.

## Requirements

### Requirement: Cadastro com e-mail
A loja DEVE permitir criar uma conta informando o e-mail, que é o identificador de login. A senha só DEVE ser definida depois que o e-mail for confirmado.

#### Scenario: Cadastro válido
- **GIVEN** não existe conta para cliente@exemplo.com
- **WHEN** o cliente se cadastra com cliente@exemplo.com
- **THEN** a conta é criada sem senha e é pedido que ele confirme o e-mail

### Requirement: E-mail único
A loja NÃO DEVE criar uma segunda conta para um e-mail já cadastrado, e DEVE oferecer o login.

#### Scenario: E-mail já cadastrado
- **GIVEN** já existe conta para cliente@exemplo.com
- **WHEN** alguém tenta se cadastrar com cliente@exemplo.com
- **THEN** nenhuma conta é criada e é oferecido o login

### Requirement: Confirmação do e-mail e definição da senha
Ao criar a conta, a loja DEVE enviar um link ao e-mail cadastrado. Abrir o link DEVE confirmar o e-mail e pedir uma senha de no mínimo 8 caracteres; definida a senha, a sessão DEVE ser iniciada.

#### Scenario: Link aberto
- **GIVEN** o cliente acabou de se cadastrar com cliente@exemplo.com
- **WHEN** ele abre o link recebido e define uma senha de 8 caracteres
- **THEN** o e-mail fica confirmado e a sessão é iniciada

#### Scenario: Senha curta
- **GIVEN** o cliente abriu o link de confirmação
- **WHEN** ele define uma senha de 7 caracteres
- **THEN** a senha é recusada e a regra é exibida

#### Scenario: Link perdido
- **GIVEN** a conta de cliente@exemplo.com não foi confirmada
- **WHEN** o cliente pede a recuperação de senha com cliente@exemplo.com
- **THEN** ele recebe um novo link para confirmar o e-mail e definir a senha

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
