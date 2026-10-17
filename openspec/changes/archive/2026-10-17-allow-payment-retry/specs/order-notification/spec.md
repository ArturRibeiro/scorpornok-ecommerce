# Spec Delta

## MODIFIED Requirements

### Requirement: Um e-mail por resultado
O cliente DEVE receber o e-mail do resultado de cada tentativa de pagamento mesmo que o primeiro envio falhe, e NÃO DEVE receber um segundo e-mail quando o mesmo resultado chega de novo. Uma falha no envio NÃO DEVE desfazer nem atrasar a atualização do status do pedido.

#### Scenario: Servidor de e-mail indisponível
- **GIVEN** o servidor de e-mail está fora do ar
- **WHEN** o pagamento do pedido A123 é aprovado
- **THEN** o pedido A123 fica confirmado imediatamente
- **AND** o cliente recebe o e-mail de confirmação quando o servidor de e-mail voltar

#### Scenario: Resultado repetido
- **GIVEN** o cliente já recebeu o e-mail de confirmação do pedido A123
- **WHEN** o resultado "aprovado" do pedido A123 chega de novo
- **THEN** nenhum novo e-mail é enviado

#### Scenario: Recusa e aprovação na nova tentativa
- **GIVEN** o cliente recebeu o e-mail da recusa da tentativa 1 do pedido A123
- **WHEN** a tentativa 2 de A123 é aprovada
- **THEN** o cliente recebe o e-mail de confirmação de A123
