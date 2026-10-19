# Spec Delta

## MODIFIED Requirements

### Requirement: Registro do pedido
A API de Orders DEVE aceitar de um cliente autenticado um pedido com endereço, itens e dados do cartão. Cada item informa o identificador do produto (numérico, o mesmo do Catalog) e a quantidade. O pedido DEVE ser persistido pendente, com o e-mail da conta e o endereço de entrega informado (rua, número, complemento, bairro, cidade, UF e CEP, no Brasil).

#### Scenario: Pedido válido
- **GIVEN** o cliente autenticado cliente@exemplo.com envia um pedido com endereço completo, um item válido e cartão
- **WHEN** o pedido é registrado
- **THEN** ele é persistido pendente, com o e-mail cliente@exemplo.com e o endereço informado

#### Scenario: Identificador de produto do Catalog
- **GIVEN** um item com o identificador de produto 42, como no Catalog
- **WHEN** o pedido é registrado
- **THEN** o item é persistido com o identificador 42

### Requirement: Rejeição de pedido inválido
A API DEVE validar o pedido antes de persistir e responder HTTP 400 com as mensagens de validação, sem persistir. O pedido é inválido sem endereço completo (rua, número, bairro, cidade, UF e CEP), com UF ou CEP inválidos, sem itens, com item de quantidade menor ou igual a zero, ou com item cujo produto não existe no Catalog.

#### Scenario: Endereço incompleto
- **GIVEN** um pedido sem cidade
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o endereço, e nada é persistido

#### Scenario: UF inválida
- **GIVEN** um pedido com a UF XX
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre a UF, e nada é persistido

#### Scenario: Pedido sem itens
- **GIVEN** um pedido sem itens
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre os itens, e nada é persistido

#### Scenario: Quantidade zero
- **GIVEN** um pedido com um item de quantidade 0
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre a quantidade

#### Scenario: Produto inexistente
- **GIVEN** o produto 999 não existe no Catalog
- **WHEN** um pedido com o produto 999 é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem citando o produto 999, e nada é persistido nem cobrado

#### Scenario: E-mail ausente ou inválido
- **GIVEN** o cliente autenticado cliente@exemplo.com envia um pedido válido sem e-mail ou com "cliente@" no corpo
- **WHEN** ele é enviado
- **THEN** o pedido é aceito com o e-mail cliente@exemplo.com

## ADDED Requirements

### Requirement: Dados do comprador no pedido
A API de Orders DEVE exigir no pedido o nome completo, o telefone e o CPF do comprador, validados com as mesmas regras do cadastro, e responder HTTP 400 sem persistir quando faltarem ou forem inválidos. Esses dados NÃO DEVEM ser guardados no pedido.

#### Scenario: CPF inválido no pedido
- **GIVEN** um pedido válido com o CPF 111.111.111-11
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o CPF, e nada é persistido nem cobrado

### Requirement: Informações da compra para o cadastro
Um pedido registrado DEVE enviar ao serviço de cadastro o cliente, o e-mail, o nome, o telefone, o CPF, o número do pedido e o endereço de entrega do pedido, de forma atômica com o registro. O envio NÃO DEVE depender do broker ou do serviço de cadastro estarem no ar no momento do registro.

#### Scenario: Pedido registrado
- **GIVEN** o cliente envia o pedido A123 com entrega no Rio de Janeiro
- **WHEN** o pedido é registrado
- **THEN** o serviço de cadastro recebe as informações do pedido A123 com o endereço do Rio de Janeiro

#### Scenario: Broker indisponível
- **GIVEN** o broker de mensagens está parado
- **WHEN** o pedido A123 é registrado
- **THEN** a resposta é HTTP 201
- **AND** as informações do pedido A123 chegam ao serviço de cadastro quando o broker volta

#### Scenario: Falha ao registrar
- **GIVEN** o banco de dados do Orders falha durante o registro do pedido
- **WHEN** o pedido é enviado
- **THEN** nenhuma informação do pedido é enviada ao serviço de cadastro
