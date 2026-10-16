# Spec Delta

## ADDED Requirements

### Requirement: Número do pedido único
A API de Orders DEVE atribuir a cada pedido registrado um número que nenhum outro pedido tenha, no formato `A` seguido de dígitos. Ela NÃO DEVE registrar um pedido com um número já usado; nesse caso, o pedido não é gravado nem cobrado.

#### Scenario: Pedidos com números diferentes
- **GIVEN** dois pedidos válidos enviados em sequência
- **WHEN** os dois são registrados
- **THEN** cada um recebe um número diferente, começando com `A` e seguido de dígitos

#### Scenario: Número já usado
- **GIVEN** já existe um pedido com o número A123456789012
- **WHEN** outro pedido recebe o mesmo número ao ser registrado
- **THEN** o registro falha, e nada é gravado nem cobrado para o segundo pedido
