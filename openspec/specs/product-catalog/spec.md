# product-catalog Specification

## Purpose

Consulta do catálogo de produtos da loja: listagem paginada e produto por id, pelo GraphQL e pelo REST do Catalog.

## Requirements

### Requirement: Listagem paginada
O catálogo DEVE listar os produtos em páginas numeradas, em ordem estável, com no máximo 20 por página, informando o total e se há página anterior e seguinte.

#### Scenario: Primeira página
- **GIVEN** o catálogo tem 281 produtos
- **WHEN** a loja pede a página 1 com 18
- **THEN** recebe 18 produtos, total 281, com página seguinte e sem anterior

#### Scenario: Acima do limite
- **GIVEN** o catálogo tem mais de 20 produtos
- **WHEN** a loja pede 50 por página
- **THEN** recebe no máximo 20

### Requirement: Produto por id
O catálogo DEVE devolver um produto pelo id, e um id inexistente NÃO DEVE gerar erro.

#### Scenario: Produto inexistente
- **GIVEN** não existe o produto 999999
- **WHEN** a loja o consulta
- **THEN** a resposta indica que não há produto, sem erro, e a loja mostra "produto não encontrado"

### Requirement: Mesmo catálogo nas duas APIs
O GraphQL e o REST do Catalog DEVEM devolver os mesmos produtos, na mesma ordem e com os mesmos valores. O REST DEVE continuar respondendo como hoje.

#### Scenario: Mesma página
- **GIVEN** o Catalog está no ar
- **WHEN** a página 2 com 18 é consultada pelas duas APIs
- **THEN** as respostas trazem os mesmos produtos, na mesma ordem e com o mesmo preço
