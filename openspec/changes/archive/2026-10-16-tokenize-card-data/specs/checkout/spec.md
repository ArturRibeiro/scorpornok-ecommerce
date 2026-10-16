# Spec Delta

## ADDED Requirements

### Requirement: Falha na tokenização do cartão
Quando o serviço de tokenização recusa o cartão ou não responde, o checkout NÃO DEVE criar o pedido e DEVE mostrar o motivo, mantendo o carrinho e os dados digitados.

#### Scenario: Tokenização indisponível
- **GIVEN** o serviço de tokenização está fora do ar
- **WHEN** o cliente aciona "Confirmar pedido" com o formulário válido
- **THEN** nenhum pedido é criado e é exibida uma mensagem de que não foi possível processar o cartão
- **AND** o carrinho e os campos preenchidos continuam como estavam

## MODIFIED Requirements

### Requirement: Dados do cartão
O checkout DEVE coletar nome do titular, número do cartão, mês e ano de validade, CVV e quantidade de parcelas (padrão 1), e DEVE enviar o titular, o número, a validade e o CVV só ao serviço de tokenização. Ele NÃO DEVE enviar o pedido com o número fora de 13 a 19 dígitos, o CVV fora de 3 ou 4 dígitos ou a validade no passado.

#### Scenario: Cartão vencido
- **GIVEN** a data atual é outubro de 2026
- **WHEN** o cliente informa a validade 09/2026 e aciona "Confirmar pedido"
- **THEN** o pedido não é enviado
- **AND** a validade é indicada como inválida

#### Scenario: Parcelas padrão
- **GIVEN** o cliente não escolheu a quantidade de parcelas
- **WHEN** o pedido é enviado
- **THEN** o pedido é enviado com 1 parcela

#### Scenario: Cartão só na tokenização
- **GIVEN** o cliente preencheu o cartão 4111111111111111 com CVV 123
- **WHEN** ele confirma o pedido
- **THEN** o número e o CVV são enviados só ao serviço de tokenização
- **AND** o pedido enviado ao serviço de pedidos leva o token, sem número nem CVV

### Requirement: Envio do pedido
Ao confirmar, o checkout DEVE tokenizar o cartão e, em seguida, enviar ao serviço de pedidos os itens, o endereço, o token do cartão e as parcelas, em nome do cliente identificado. Enquanto a tokenização e o envio estão em andamento, o botão de confirmação DEVE ficar desabilitado.

#### Scenario: Envio em andamento
- **GIVEN** o formulário está válido
- **WHEN** o cliente aciona "Confirmar pedido"
- **THEN** o botão fica desabilitado e indica o envio até a resposta chegar

#### Scenario: E-mail no pedido
- **GIVEN** o cliente está identificado com cliente@exemplo.com
- **WHEN** ele confirma um pedido válido
- **THEN** o pedido é registrado na conta dele, com o e-mail cliente@exemplo.com
