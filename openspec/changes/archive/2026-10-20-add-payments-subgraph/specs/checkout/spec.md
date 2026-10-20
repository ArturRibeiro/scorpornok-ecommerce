# Spec Delta

## MODIFIED Requirements

### Requirement: Dados do cartão
O checkout DEVE coletar nome do titular, número do cartão, mês e ano de validade, CVV e quantidade de parcelas (padrão 1), e DEVE enviar o titular, o número, a validade e o CVV só ao serviço de tokenização, pelo ponto de entrada único, sem que esses dados fiquem registrados no caminho. Ele NÃO DEVE enviar o pedido com o número fora de 13 a 19 dígitos, o CVV fora de 3 ou 4 dígitos ou a validade no passado.

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
- **THEN** o número e o CVV são enviados só ao serviço de tokenização, pelo ponto de entrada
- **AND** o pedido enviado ao serviço de pedidos leva o token, sem número nem CVV
