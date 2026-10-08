# Spec Delta

## Purpose

Define o contrato da API de Orders para registrar um pedido: que dados ela aceita, como valida e o que responde a quem a chama, inclusive a loja rodando no navegador.

## ADDED Requirements

### Requirement: Registro do pedido
A API de Orders DEVE aceitar um pedido com identificador do cliente, endereço de entrega, itens e dados do cartão. Cada item tem identificador do produto (numérico, o mesmo do Catalog), nome, imagem, preço unitário, desconto e quantidade. O pedido aceito DEVE ser persistido com status pendente.

#### Scenario: Pedido válido
- **GIVEN** um pedido com cliente, endereço completo, um item com preço e quantidade maiores que zero e dados do cartão
- **WHEN** o pedido é enviado à API de Orders
- **THEN** o pedido é persistido com status pendente

#### Scenario: Identificador de produto do Catalog
- **GIVEN** um item cujo identificador de produto é o número 42, como no Catalog
- **WHEN** o pedido é enviado
- **THEN** o pedido é aceito e o item é persistido com o identificador 42

### Requirement: Cliente do pedido
O pedido persistido DEVE pertencer ao cliente informado no envio. A API NÃO DEVE gerar outro identificador de cliente.

#### Scenario: Cliente preservado
- **GIVEN** um pedido válido enviado com o identificador de cliente C
- **WHEN** o pedido é persistido
- **THEN** o cliente do pedido é C

### Requirement: Resposta de pedido registrado
Quando o pedido é registrado, a API DEVE responder com sucesso de criação (HTTP 201). O corpo DEVE trazer o número do pedido, o status e o total, que é a soma de preço unitário vezes quantidade dos itens.

#### Scenario: Corpo da resposta
- **GIVEN** um pedido válido com 2 unidades a $10.00 e 1 unidade a $5.00
- **WHEN** o pedido é registrado
- **THEN** a resposta é HTTP 201
- **AND** o corpo traz um número de pedido não vazio, o status pendente e o total 25.00

### Requirement: Rejeição de pedido inválido
A API DEVE validar o pedido antes de persistir. Um pedido inválido NÃO DEVE ser persistido, e a API DEVE responder HTTP 400 com a lista de mensagens de validação. O pedido é inválido sem endereço completo, sem itens, ou com item de preço ou quantidade menor ou igual a zero.

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

### Requirement: Pedido pendente sem pagamento
O registro de um pedido pendente NÃO DEVE exigir um identificador de pagamento, porque o pagamento acontece depois da criação.

#### Scenario: Pedido novo sem pagamento
- **GIVEN** um pedido válido, ainda sem pagamento processado
- **WHEN** o pedido é enviado
- **THEN** o pedido é registrado com status pendente e sem identificador de pagamento

### Requirement: Acesso a partir da loja
A API de Orders DEVE aceitar chamadas feitas pelo navegador a partir das origens configuradas da loja e DEVE recusar as demais origens.

#### Scenario: Origem da loja
- **GIVEN** a origem `http://localhost:3000` está configurada
- **WHEN** o navegador envia um pedido a partir dessa origem
- **THEN** a requisição é permitida, inclusive a verificação prévia (preflight) do navegador

#### Scenario: Origem não configurada
- **GIVEN** a origem `http://exemplo.com` não está configurada
- **WHEN** o navegador tenta enviar um pedido a partir dela
- **THEN** o navegador bloqueia a requisição, porque a API não libera essa origem
