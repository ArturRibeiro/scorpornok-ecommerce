# Spec Delta

## ADDED Requirements

### Requirement: Dados do item vindos do Catalog
A API de Orders DEVE registrar cada item com o preço unitário, o nome e a imagem do produto no Catalog no momento do registro, ignorando os valores desses campos enviados no pedido. O total do pedido e o valor cobrado DEVEM usar esse preço.

#### Scenario: Preço adulterado no pedido
- **GIVEN** o produto 42 custa $10.00 no Catalog
- **WHEN** o cliente envia um pedido com 2 unidades do produto 42 a $0.01
- **THEN** o item é registrado a $10.00, o total é 20.00 e o pagamento é solicitado no valor de 20.00

#### Scenario: Nome e imagem do Catalog
- **GIVEN** o produto 42 se chama "Camiseta" no Catalog
- **WHEN** o cliente envia um pedido com o produto 42 chamado "Brinde"
- **THEN** o item é registrado com o nome "Camiseta" e a imagem do Catalog

### Requirement: Catalog indisponível no registro
Quando o Catalog não puder ser consultado, a API de Orders DEVE responder HTTP 503, sem persistir o pedido nem solicitar pagamento.

#### Scenario: Catalog fora do ar
- **GIVEN** o Catalog está parado
- **WHEN** um pedido válido é enviado
- **THEN** a resposta é HTTP 503, e nada é persistido nem cobrado

## MODIFIED Requirements

### Requirement: Registro do pedido
A API de Orders DEVE aceitar de um cliente autenticado um pedido com endereço, itens e dados do cartão. Cada item informa o identificador do produto (numérico, o mesmo do Catalog) e a quantidade. O pedido DEVE ser persistido pendente, com o e-mail da conta.

#### Scenario: Pedido válido
- **GIVEN** o cliente autenticado cliente@exemplo.com envia um pedido com endereço completo, um item válido e cartão
- **WHEN** o pedido é registrado
- **THEN** ele é persistido pendente, com o e-mail cliente@exemplo.com

#### Scenario: Identificador de produto do Catalog
- **GIVEN** um item com o identificador de produto 42, como no Catalog
- **WHEN** o pedido é registrado
- **THEN** o item é persistido com o identificador 42

### Requirement: Rejeição de pedido inválido
A API DEVE validar o pedido antes de persistir e responder HTTP 400 com as mensagens de validação, sem persistir. O pedido é inválido sem endereço completo, sem itens, com item de quantidade menor ou igual a zero, ou com item cujo produto não existe no Catalog.

#### Scenario: Endereço incompleto
- **GIVEN** um pedido sem cidade
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o endereço, e nada é persistido

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
