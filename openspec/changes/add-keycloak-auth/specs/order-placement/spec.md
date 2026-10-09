# Spec Delta

## ADDED Requirements

### Requirement: Pedido exige autenticação
A API de Orders DEVE responder HTTP 401 a um pedido sem credencial ou com credencial inválida ou expirada, sem persistir o pedido nem solicitar pagamento.

#### Scenario: Sem credencial
- **GIVEN** um pedido válido sem credencial
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 401, e nada é persistido nem cobrado

#### Scenario: Credencial expirada
- **GIVEN** um pedido válido com credencial expirada
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 401

### Requirement: Pedido exige e-mail confirmado
A API de Orders DEVE responder HTTP 403 a um cliente autenticado cujo e-mail não foi confirmado, sem persistir o pedido.

#### Scenario: E-mail não confirmado
- **GIVEN** um cliente autenticado com e-mail não confirmado
- **WHEN** ele envia um pedido válido
- **THEN** a resposta é HTTP 403 e nada é persistido

## MODIFIED Requirements

### Requirement: Registro do pedido
A API de Orders DEVE aceitar de um cliente autenticado um pedido com endereço, itens e dados do cartão. Cada item tem identificador do produto (numérico, o mesmo do Catalog), nome, imagem, preço unitário, desconto e quantidade. O pedido DEVE ser persistido pendente, com o e-mail da conta.

#### Scenario: Pedido válido
- **GIVEN** o cliente autenticado cliente@exemplo.com envia um pedido com endereço completo, um item válido e cartão
- **WHEN** o pedido é registrado
- **THEN** ele é persistido pendente, com o e-mail cliente@exemplo.com

#### Scenario: Identificador de produto do Catalog
- **GIVEN** um item com o identificador de produto 42, como no Catalog
- **WHEN** o pedido é registrado
- **THEN** o item é persistido com o identificador 42

### Requirement: Cliente do pedido
O pedido DEVE pertencer ao cliente autenticado que o enviou, ignorando qualquer identificador de cliente no corpo.

#### Scenario: Cliente preservado
- **GIVEN** o cliente autenticado C envia um pedido com o identificador de cliente D no corpo
- **WHEN** o pedido é persistido
- **THEN** o cliente do pedido é C

### Requirement: Rejeição de pedido inválido
A API DEVE validar o pedido antes de persistir e responder HTTP 400 com as mensagens de validação, sem persistir. O pedido é inválido sem endereço completo, sem itens, ou com item de preço ou quantidade menor ou igual a zero.

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

#### Scenario: E-mail ausente ou inválido
- **GIVEN** o cliente autenticado cliente@exemplo.com envia um pedido válido sem e-mail ou com "cliente@" no corpo
- **WHEN** ele é enviado
- **THEN** o pedido é aceito com o e-mail cliente@exemplo.com

### Requirement: Notificação do resultado à loja
Depois de atualizar o pedido com o resultado do pagamento, a API de Orders DEVE enviar o número do pedido e se foi aprovado a quem o acompanha; quem começar a acompanhar um pedido já resolvido DEVE receber o resultado na hora. Só o dono autenticado, a partir das origens configuradas da loja, DEVE poder acompanhar o pedido.

#### Scenario: Resultado enviado a quem acompanha
- **GIVEN** o dono acompanha o pedido pendente A123
- **WHEN** o pagamento de A123 é aprovado
- **THEN** a loja recebe A123 com pagamento aprovado

#### Scenario: Acompanhamento iniciado depois do resultado
- **GIVEN** o pedido A123 já está com falha no pagamento
- **WHEN** o dono começa a acompanhar A123
- **THEN** a loja recebe na hora A123 com pagamento recusado

#### Scenario: Origem não configurada
- **GIVEN** a origem `http://exemplo.com` não está configurada
- **WHEN** o navegador tenta acompanhar um pedido a partir dela
- **THEN** a conexão é recusada

#### Scenario: Sem credencial
- **GIVEN** a loja não envia credencial
- **WHEN** ela tenta acompanhar A123
- **THEN** a conexão é recusada

#### Scenario: Pedido de outro cliente
- **GIVEN** A123 pertence ao cliente C
- **WHEN** o cliente autenticado D tenta acompanhar A123
- **THEN** o acompanhamento é recusado, sem mensagem sobre A123
