# Spec Delta

## ADDED Requirements

### Requirement: Tokenização pelo ponto de entrada
A loja DEVE tokenizar o cartão pelo ponto de entrada único, com as mesmas regras e os mesmos erros por campo de hoje. O ponto de entrada NÃO DEVE registrar nem devolver o número, a validade ou o CVV em logs, telemetria ou mensagens de erro, nem quando a tokenização falha.

#### Scenario: Cartão válido
- **GIVEN** o cliente preencheu um cartão válido no checkout
- **WHEN** confirma o pedido
- **THEN** o cartão é tokenizado pelo ponto de entrada e o pedido segue só com o token

#### Scenario: Cartão inválido
- **GIVEN** o cliente informou o CVV 12
- **WHEN** confirma o pedido
- **THEN** a resposta traz o erro no campo CVV e nada é enviado ao serviço de pedidos

#### Scenario: Falha sem vazar o cartão
- **GIVEN** o serviço de tokenização está fora do ar
- **WHEN** a loja tokeniza o cartão 4111111111111111
- **THEN** a loja informa que não foi possível validar o cartão
- **AND** o número e o CVV não aparecem nos logs nem na resposta do ponto de entrada
