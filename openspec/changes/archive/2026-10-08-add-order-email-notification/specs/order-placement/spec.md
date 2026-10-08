# Spec Delta

## MODIFIED Requirements

### Requirement: Registro do pedido
A API de Orders DEVE aceitar um pedido com identificador do cliente, e-mail do cliente, endereço de entrega, itens e dados do cartão. Cada item tem identificador do produto (numérico, o mesmo do Catalog), nome, imagem, preço unitário, desconto e quantidade. O pedido aceito DEVE ser persistido com status pendente e com o e-mail informado.

#### Scenario: Pedido válido
- **GIVEN** um pedido com cliente, e-mail válido, endereço completo, um item com preço e quantidade maiores que zero e dados do cartão
- **WHEN** o pedido é enviado à API de Orders
- **THEN** o pedido é persistido com status pendente e com o e-mail informado

#### Scenario: Identificador de produto do Catalog
- **GIVEN** um item cujo identificador de produto é o número 42, como no Catalog
- **WHEN** o pedido é enviado
- **THEN** o pedido é aceito e o item é persistido com o identificador 42

### Requirement: Rejeição de pedido inválido
A API DEVE validar o pedido antes de persistir. Um pedido inválido NÃO DEVE ser persistido, e a API DEVE responder HTTP 400 com a lista de mensagens de validação. O pedido é inválido sem e-mail ou com e-mail em formato inválido, sem endereço completo, sem itens, ou com item de preço ou quantidade menor ou igual a zero.

#### Scenario: Endereço incompleto
- **GIVEN** um pedido sem cidade no endereço
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o endereço
- **AND** nenhum pedido é persistido

#### Scenario: Pedido sem itens
- **GIVEN** um pedido com a lista de itens vazia
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre os itens
- **AND** nenhum pedido é persistido

#### Scenario: Quantidade zero
- **GIVEN** um pedido com um item de quantidade 0
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre a quantidade

#### Scenario: E-mail ausente ou inválido
- **GIVEN** um pedido sem e-mail ou com o e-mail "cliente@"
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o e-mail
- **AND** nenhum pedido é persistido
