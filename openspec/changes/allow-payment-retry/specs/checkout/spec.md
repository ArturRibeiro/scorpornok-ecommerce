# Spec Delta

## ADDED Requirements

### Requirement: Nova tentativa com outro cartão
Quando o pagamento for recusado, o checkout DEVE oferecer pagar o mesmo pedido com outro cartão, sem refazer o carrinho, e voltar a acompanhar o resultado. Esgotadas as tentativas, NÃO DEVE oferecer outra.

#### Scenario: Pagar com outro cartão
- **GIVEN** o pagamento do pedido A123 foi recusado
- **WHEN** o cliente informa outro cartão e confirma
- **THEN** a confirmação volta a indicar pagamento em processamento para A123

#### Scenario: Tentativas esgotadas
- **GIVEN** o pedido A123 teve 3 pagamentos recusados
- **WHEN** chega a terceira recusa
- **THEN** a confirmação mostra a recusa sem a opção de tentar outro cartão
- **AND** o carrinho continua com os itens, para um novo pedido

## MODIFIED Requirements

### Requirement: Pedido registrado
Quando o serviço de pedidos aceita o pedido, o checkout DEVE mostrar o número do pedido e indicar que o pagamento está em processamento. O carrinho DEVE ser esvaziado só quando o pagamento for aprovado ou ficar pendente, e mantido quando for recusado.

#### Scenario: Pedido aceito
- **GIVEN** o formulário está válido e o serviço de pedidos está no ar
- **WHEN** o cliente confirma o pedido e o serviço o aceita
- **THEN** é exibida uma confirmação com o número do pedido e a indicação de que o pagamento está em processamento
- **AND** o carrinho continua com os itens

#### Scenario: Carrinho após o resultado
- **GIVEN** o pedido foi aceito e o carrinho tem 2 itens
- **WHEN** o pagamento é aprovado, ou passam 10 segundos sem resultado
- **THEN** o carrinho fica vazio, inclusive no contador do cabeçalho

#### Scenario: Carrinho na recusa
- **GIVEN** o pedido foi aceito e o carrinho tem 2 itens
- **WHEN** o pagamento é recusado
- **THEN** o carrinho continua com os 2 itens
