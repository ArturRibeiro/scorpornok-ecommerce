# Spec Delta

## ADDED Requirements

### Requirement: Pedidos pelo ponto de entrada
A loja DEVE registrar pedidos, pagar de novo um pedido recusado e consultar os próprios pedidos pelo ponto de entrada único, com as mesmas regras de hoje. O acompanhamento do pagamento NÃO DEVE mudar.

#### Scenario: Compra pelo ponto de entrada
- **GIVEN** o cliente está identificado e o carrinho tem itens
- **WHEN** confirma o checkout
- **THEN** o pedido é registrado pelo ponto de entrada, a confirmação acompanha o pagamento como antes e o navegador não chama a API de pedidos diretamente

#### Scenario: Meus pedidos pelo ponto de entrada
- **GIVEN** o cliente tem pedidos
- **WHEN** abre "Meus pedidos" e um pedido
- **THEN** vê a mesma lista e o mesmo detalhe de antes

### Requirement: Erros do pedido no ponto de entrada
Os desfechos que não registram ou não alteram o pedido DEVEM chegar à loja como erro com código e a mensagem de hoje: pedido inválido, catálogo indisponível, pedido não encontrado e pagamento que não pode ser refeito.

#### Scenario: Pedido inválido
- **GIVEN** o cliente está identificado
- **WHEN** registra um pedido com CPF inválido
- **THEN** a resposta traz o código de validação com a mensagem "CPF inválido." e nada é registrado

#### Scenario: Catálogo indisponível
- **GIVEN** o Catalog está fora do ar
- **WHEN** o cliente registra um pedido
- **THEN** a resposta traz o código de indisponibilidade, nada é registrado nem cobrado, e a loja mantém o carrinho

#### Scenario: Pedido de outro cliente
- **GIVEN** o pedido A999 é de outro cliente
- **WHEN** o cliente consulta A999 ou tenta pagá-lo de novo
- **THEN** a resposta é "pedido não encontrado", como para um número inexistente
