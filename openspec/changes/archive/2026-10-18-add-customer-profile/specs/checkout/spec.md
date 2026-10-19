# Spec Delta

## ADDED Requirements

### Requirement: Dados do comprador
O checkout DEVE coletar nome completo, telefone e CPF, obrigatórios e validados com as mesmas regras do cadastro, e NÃO DEVE enviar o pedido com algum deles vazio ou inválido. Quando o cliente tem cadastro, esses campos DEVEM abrir preenchidos; o que o cliente mudar ali DEVE valer só para aquele pedido, sem alterar um cadastro existente.

#### Scenario: Dados do cadastro
- **GIVEN** o cadastro do cliente tem o nome Maria Souza, o telefone (21) 99999-8888 e o CPF 529.982.247-25
- **WHEN** ele abre o checkout
- **THEN** nome, telefone e CPF já vêm preenchidos com esses valores

#### Scenario: CPF inválido no checkout
- **GIVEN** o cliente informou o CPF 111.111.111-11
- **WHEN** ele aciona "Confirmar pedido"
- **THEN** o pedido não é enviado e o CPF é indicado como inválido

#### Scenario: Sem cadastro
- **GIVEN** o cliente não tem cadastro, ou o serviço de cadastro está fora do ar
- **WHEN** ele abre o checkout
- **THEN** nome, telefone e CPF vêm em branco e precisam ser preenchidos para enviar o pedido

## MODIFIED Requirements

### Requirement: Envio do pedido
Ao confirmar, o checkout DEVE tokenizar o cartão e, em seguida, enviar ao serviço de pedidos os itens, o endereço de entrega, os dados do comprador, o token do cartão e as parcelas, em nome do cliente identificado. Enquanto a tokenização e o envio estão em andamento, o botão de confirmação DEVE ficar desabilitado.

#### Scenario: Envio em andamento
- **GIVEN** o formulário está válido
- **WHEN** o cliente aciona "Confirmar pedido"
- **THEN** o botão fica desabilitado e indica o envio até a resposta chegar

#### Scenario: E-mail no pedido
- **GIVEN** o cliente está identificado com cliente@exemplo.com
- **WHEN** ele confirma um pedido válido
- **THEN** o pedido é registrado na conta dele, com o e-mail cliente@exemplo.com

### Requirement: Endereço de entrega
O checkout DEVE coletar rua, número, complemento, bairro, cidade, UF e CEP, todos obrigatórios exceto o complemento e validados com as regras do cadastro, e NÃO DEVE enviar o pedido com algum deles vazio ou inválido. Quando o cliente tem cadastro, esses campos DEVEM abrir preenchidos com o endereço dele; o que o cliente mudar ali DEVE valer só para aquele pedido, sem alterar um cadastro existente.

#### Scenario: Campo de endereço vazio
- **GIVEN** o cliente preencheu tudo menos a cidade
- **WHEN** ele aciona "Confirmar pedido"
- **THEN** o pedido não é enviado
- **AND** o campo cidade é indicado como obrigatório

#### Scenario: CEP inválido
- **GIVEN** o cliente informou o CEP 1234-567
- **WHEN** ele aciona "Confirmar pedido"
- **THEN** o pedido não é enviado e o CEP é indicado como inválido

#### Scenario: Endereço do cadastro
- **GIVEN** o cadastro do cliente tem o endereço Rua das Flores, 100, Centro, São Paulo, SP, CEP 01000-000
- **WHEN** ele abre o checkout
- **THEN** rua, número, bairro, cidade, UF e CEP já vêm preenchidos com esse endereço

#### Scenario: Endereço alterado no checkout
- **GIVEN** o checkout abriu com o endereço do cadastro
- **WHEN** o cliente troca a cidade e confirma o pedido
- **THEN** o pedido é enviado com a cidade digitada
- **AND** o endereço do cadastro continua o mesmo

#### Scenario: Sem endereço ou cadastro indisponível
- **GIVEN** o cliente não tem endereço no cadastro, ou o serviço de cadastro está fora do ar
- **WHEN** ele abre o checkout
- **THEN** os campos de endereço vêm em branco e o checkout funciona normalmente
