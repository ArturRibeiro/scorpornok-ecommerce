# Spec Delta

## ADDED Requirements

### Requirement: Login exigido no checkout
O checkout DEVE exigir um cliente identificado. Sem sessão, o cliente DEVE ser levado ao login, com a opção de se cadastrar, e voltar ao checkout com o carrinho como estava.

#### Scenario: Checkout sem login
- **GIVEN** o cliente não está identificado e tem 2 itens no carrinho
- **WHEN** ele aciona "Proceed to Checkout" e entra
- **THEN** ele volta ao checkout com os mesmos 2 itens

#### Scenario: Sessão expirada ao confirmar
- **GIVEN** a sessão do cliente expirou sem poder ser renovada
- **WHEN** ele aciona "Place order"
- **THEN** o pedido não é registrado e ele é levado ao login, mantendo o carrinho

### Requirement: E-mail da conta no checkout
O checkout DEVE exibir o e-mail da conta como destino do resultado do pagamento, sem campo para digitar outro.

#### Scenario: E-mail exibido
- **GIVEN** o cliente está identificado com cliente@exemplo.com
- **WHEN** ele abre o checkout
- **THEN** cliente@exemplo.com aparece como destino do resultado, sem campo de e-mail

## MODIFIED Requirements

### Requirement: Envio do pedido
Ao confirmar, o checkout DEVE enviar ao serviço de pedidos os itens, o endereço e os dados do cartão, em nome do cliente identificado. Enquanto o envio está em andamento, o botão de confirmação DEVE ficar desabilitado.

#### Scenario: Envio em andamento
- **GIVEN** o formulário está válido
- **WHEN** o cliente aciona "Place order"
- **THEN** o botão fica desabilitado e indica o envio até a resposta chegar

#### Scenario: E-mail no pedido
- **GIVEN** o cliente está identificado com cliente@exemplo.com
- **WHEN** ele confirma um pedido válido
- **THEN** o pedido é registrado na conta dele, com o e-mail cliente@exemplo.com

## REMOVED Requirements

### Requirement: Identificação anônima do cliente
**Reason**: O cliente passa a ser identificado pela conta.
**Migration**: Ver "Login exigido no checkout".

### Requirement: E-mail do cliente
**Reason**: O e-mail passa a ser o da conta, já confirmado.
**Migration**: Ver "E-mail da conta no checkout".
