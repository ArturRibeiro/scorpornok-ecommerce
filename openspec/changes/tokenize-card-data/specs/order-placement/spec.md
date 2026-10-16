# Spec Delta

## ADDED Requirements

### Requirement: Pedido sem dados do cartão
A API de Orders DEVE aceitar o pagamento do pedido só como token do cartão e parcelas, e NÃO DEVE persistir nem repassar número, validade ou CVV, mesmo que venham no corpo.

#### Scenario: Cartão no corpo
- **GIVEN** um pedido válido enviado com o token T e também com número e CVV de cartão no corpo
- **WHEN** o pedido é registrado
- **THEN** a solicitação de pagamento leva só o token T e as parcelas
- **AND** nem o número nem o CVV ficam gravados no banco do Orders

#### Scenario: Pagamento sem token
- **GIVEN** um pedido válido com parcelas, mas sem token do cartão
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o cartão, e nada é persistido

## MODIFIED Requirements

### Requirement: Solicitação de pagamento
Depois de persistir um pedido válido, a API de Orders DEVE solicitar o pagamento ao contexto Payments com o número do pedido, o token do cartão, as parcelas e o total calculado pelo próprio pedido, ignorando qualquer valor de cobrança enviado pelo cliente. Um pedido inválido NÃO DEVE gerar solicitação.

#### Scenario: Pedido válido solicita pagamento
- **GIVEN** um pedido válido com 2 unidades a $10.00 e 1 unidade a $5.00 e o token T
- **WHEN** o pedido é registrado
- **THEN** o pagamento do pedido é solicitado com o token T no valor de 25.00

#### Scenario: Pedido inválido não solicita pagamento
- **GIVEN** um pedido sem itens
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 e nenhum pagamento é solicitado

#### Scenario: Payments fora do ar
- **GIVEN** o contexto Payments está parado
- **WHEN** um pedido válido é enviado
- **THEN** a resposta é HTTP 201 com o status pendente, como de costume
