# Spec Delta

## ADDED Requirements

### Requirement: E-mail do cliente
O checkout DEVE coletar o e-mail do cliente, obrigatório e em formato válido, e DEVE informar que o resultado do pagamento será enviado para ele. Ele NÃO DEVE enviar o pedido enquanto o e-mail estiver vazio ou inválido.

#### Scenario: E-mail vazio
- **GIVEN** o cliente preencheu tudo menos o e-mail
- **WHEN** ele aciona "Place order"
- **THEN** o pedido não é enviado
- **AND** o campo de e-mail é indicado como obrigatório

#### Scenario: E-mail inválido
- **GIVEN** o cliente digitou "cliente@" no e-mail
- **WHEN** ele aciona "Place order"
- **THEN** o pedido não é enviado
- **AND** o campo de e-mail é indicado como inválido

## MODIFIED Requirements

### Requirement: Envio do pedido
Ao confirmar, o checkout DEVE enviar ao serviço de pedidos os itens, o endereço, os dados do cartão, o e-mail e o identificador do cliente. Enquanto o envio está em andamento, o botão de confirmação DEVE ficar desabilitado para impedir envios duplicados.

#### Scenario: Envio em andamento
- **GIVEN** o formulário está válido
- **WHEN** o cliente aciona "Place order"
- **THEN** o botão fica desabilitado e indica que o pedido está sendo enviado até a resposta chegar

#### Scenario: E-mail no pedido
- **GIVEN** o formulário está válido com o e-mail cliente@exemplo.com
- **WHEN** o cliente aciona "Place order"
- **THEN** o pedido enviado ao serviço de pedidos contém o e-mail cliente@exemplo.com

### Requirement: Acompanhamento do pagamento
Depois que o pedido é aceito, o checkout DEVE receber do serviço de pedidos a mensagem com o resultado do pagamento, sem consultar o pedido periodicamente. Se o pagamento for aprovado, DEVE mostrar que o pedido foi concluído com sucesso; se for recusado, DEVE mostrar que o pagamento foi recusado. Se a mensagem não chegar em até 10 segundos, ou se a conexão para recebê-la não puder ser aberta, o checkout DEVE informar que o pagamento está pendente e que o resultado será enviado para o e-mail do cliente.

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido foi aceito e a confirmação indica pagamento em processamento
- **WHEN** chega a mensagem de que o pagamento do pedido foi aprovado
- **THEN** a confirmação mostra que o pedido foi concluído com sucesso
- **AND** o checkout não fez consultas periódicas ao pedido

#### Scenario: Pagamento recusado
- **GIVEN** o pedido foi aceito e a confirmação indica pagamento em processamento
- **WHEN** chega a mensagem de que o pagamento do pedido foi recusado
- **THEN** a confirmação mostra que o pagamento foi recusado

#### Scenario: Mensagem não chega
- **GIVEN** o pedido foi aceito com o e-mail cliente@exemplo.com e o pagamento não é processado
- **WHEN** passam 10 segundos sem a mensagem do resultado
- **THEN** o checkout informa que o pagamento está pendente e que o resultado será enviado para cliente@exemplo.com
