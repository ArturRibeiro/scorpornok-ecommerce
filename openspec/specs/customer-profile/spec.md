# customer-profile Specification

## Purpose

Cadastro do cliente logado na loja: nome completo, telefone, CPF e um endereço de entrega, que o próprio cliente vê e edita depois do login e que a loja reaproveita no checkout.

## Requirements

### Requirement: Acesso ao cadastro
A loja DEVE oferecer a página "Meu cadastro" a partir do menu da conta. Só um cliente identificado, com e-mail confirmado, DEVE ver ou alterar um cadastro; sem sessão, a loja DEVE pedir o login.

#### Scenario: Cliente identificado
- **GIVEN** o cliente está identificado com cliente@exemplo.com
- **WHEN** ele aciona "Meu cadastro" no menu da conta
- **THEN** a página do cadastro é exibida com o e-mail cliente@exemplo.com, só para leitura

#### Scenario: Sem sessão
- **GIVEN** não há cliente identificado
- **WHEN** alguém abre o endereço de "Meu cadastro"
- **THEN** é pedido o login e nenhum dado de cadastro é exibido

### Requirement: Cadastro só do dono
O serviço de cadastro DEVE identificar o cliente pela conta da sessão e NÃO DEVE permitir ler ou alterar o cadastro de outra conta.

#### Scenario: Outra conta
- **GIVEN** o cliente A e o cliente B têm cadastro
- **WHEN** o cliente B consulta ou salva o cadastro
- **THEN** ele vê e altera só o próprio cadastro, e o de A não muda

### Requirement: Cadastro vazio
O cliente que ainda não salvou o cadastro DEVE ver o formulário com os campos em branco, sem erro.

#### Scenario: Primeiro acesso
- **GIVEN** o cliente nunca salvou o cadastro
- **WHEN** ele abre "Meu cadastro"
- **THEN** o formulário é exibido com todos os campos em branco

### Requirement: Salvar o cadastro
O cliente DEVE poder salvar nome completo, telefone, CPF e endereço de entrega (rua, número, complemento, bairro, cidade, UF e CEP). Todos os campos DEVEM ser obrigatórios, exceto o complemento; faltando algum, nada DEVE ser salvo e cada campo faltante DEVE ser indicado. Salvar DEVE substituir o cadastro anterior, e a loja DEVE confirmar o salvamento.

#### Scenario: Cadastro completo
- **GIVEN** o cliente está em "Meu cadastro"
- **WHEN** ele informa nome, telefone, CPF e endereço válidos, sem complemento, e salva
- **THEN** a loja confirma o salvamento
- **AND** ao abrir a página de novo, os dados salvos são exibidos

#### Scenario: Campos obrigatórios faltando
- **GIVEN** o cliente está em "Meu cadastro"
- **WHEN** ele informa só o nome, a rua e o CEP e salva
- **THEN** nada é salvo e telefone, CPF, número, bairro, cidade e UF são indicados como obrigatórios

### Requirement: Validação dos campos
Um campo DEVE ser recusado quando inválido: nome com menos de 2 palavras, telefone fora de 10 ou 11 dígitos com DDD, CPF com dígitos verificadores errados ou todos iguais, UF que não seja de um estado brasileiro, CEP fora de 8 dígitos. Com erro, nada DEVE ser salvo e cada campo inválido DEVE ser indicado.

#### Scenario: CPF inválido
- **GIVEN** o cliente está em "Meu cadastro"
- **WHEN** ele informa o CPF 111.111.111-11 e salva
- **THEN** nada é salvo e o campo CPF é indicado como inválido

#### Scenario: Pontuação aceita
- **GIVEN** o cliente está em "Meu cadastro"
- **WHEN** ele informa o CPF 529.982.247-25, o telefone (21) 99999-8888, o CEP 20040-020 e os demais campos e salva
- **THEN** o cadastro é salvo, com os mesmos valores exibidos ao abrir a página de novo

### Requirement: Falha ao salvar
Quando o serviço de cadastro não responde, a loja DEVE mostrar que não foi possível salvar e DEVE manter os dados digitados.

#### Scenario: Serviço fora do ar
- **GIVEN** o serviço de cadastro está fora do ar
- **WHEN** o cliente salva o cadastro
- **THEN** é exibida a mensagem de que não foi possível salvar
- **AND** os campos continuam com o que ele digitou

### Requirement: Cadastro criado na primeira compra
Quando um cliente sem cadastro registra um pedido, o serviço de cadastro DEVE criar o cadastro com o nome, o telefone e o CPF informados no pedido e com o endereço de entrega do pedido. A compra NÃO DEVE alterar um cadastro que já existe.

#### Scenario: Primeira compra
- **GIVEN** o cliente nunca salvou o cadastro
- **WHEN** ele registra o pedido A123 com nome, telefone, CPF e entrega no Rio de Janeiro
- **THEN** "Meu cadastro" passa a exibir esse nome, telefone, CPF e o endereço do Rio de Janeiro

#### Scenario: Cadastro existente
- **GIVEN** o cadastro do cliente tem endereço em São Paulo
- **WHEN** ele registra o pedido A123 com entrega no Rio de Janeiro e outro telefone
- **THEN** o cadastro continua com o endereço de São Paulo e o telefone anterior

### Requirement: Histórico de endereços de entrega
O serviço de cadastro DEVE guardar, para cada pedido registrado, o endereço de entrega do pedido ligado ao cliente e ao número do pedido. O mesmo pedido recebido de novo NÃO DEVE gerar outro registro.

#### Scenario: Entrega em outro endereço
- **GIVEN** o cadastro do cliente tem endereço em São Paulo
- **WHEN** ele registra o pedido A123 com entrega no Rio de Janeiro
- **THEN** o histórico do cliente ganha o endereço do Rio de Janeiro com o pedido A123

#### Scenario: Pedido recebido de novo
- **GIVEN** o histórico já tem o endereço do pedido A123
- **WHEN** as informações do pedido A123 chegam de novo
- **THEN** o histórico continua com um único registro do pedido A123

#### Scenario: Serviço de cadastro fora do ar
- **GIVEN** o serviço de cadastro está parado
- **WHEN** o cliente registra o pedido A123
- **THEN** o pedido é registrado normalmente
- **AND** o endereço do pedido A123 entra no histórico, e o cadastro é criado se for a primeira compra, quando o serviço volta
