# Spec Delta

## Purpose

Ponto de entrada GraphQL único da loja: reúne as consultas das APIs migradas e as protege de consultas abusivas.

## ADDED Requirements

### Requirement: Ponto de entrada único
A loja DEVE ler o catálogo por um único endereço GraphQL, que encaminha cada consulta à API dona dos dados. O que o cliente vê NÃO DEVE mudar.

#### Scenario: Home pelo ponto de entrada
- **GIVEN** o ponto de entrada e o Catalog estão no ar
- **WHEN** o cliente abre a home e um produto
- **THEN** vê a mesma grade, paginação e página de produto de antes, sem chamada direta ao Catalog

### Requirement: Origens permitidas
O ponto de entrada DEVE aceitar chamadas do navegador só das origens configuradas da loja.

#### Scenario: Outra origem
- **GIVEN** uma origem não configurada
- **WHEN** o navegador chama o ponto de entrada a partir dela
- **THEN** a resposta é bloqueada

### Requirement: Limite das consultas
O ponto de entrada DEVE recusar consultas acima da profundidade ou do custo configurados, sem consultar as APIs.

#### Scenario: Consulta profunda demais
- **GIVEN** um limite de profundidade configurado
- **WHEN** chega uma consulta mais profunda
- **THEN** a resposta traz erro de limite e nenhuma API é consultada

### Requirement: Introspecção só em desenvolvimento
O ponto de entrada DEVE responder à introspecção do schema só em ambiente de desenvolvimento.

#### Scenario: Fora de desenvolvimento
- **GIVEN** o ponto de entrada roda em produção
- **WHEN** chega uma consulta de introspecção
- **THEN** ela é recusada

### Requirement: API fora do ar
Com a API dona de uma consulta fora, o ponto de entrada DEVE devolver erro só para essa consulta e continuar no ar.

#### Scenario: Catalog fora
- **GIVEN** o Catalog está fora do ar
- **WHEN** a loja consulta os produtos
- **THEN** a resposta traz erro e a loja oferece tentar de novo

### Requirement: Saúde do ponto de entrada
O ponto de entrada DEVE indicar que está vivo e, à parte, pronto, com o schema composto carregado. A prontidão NÃO DEVE depender das APIs.

#### Scenario: Pronto com o Catalog fora
- **GIVEN** o ponto de entrada subiu e o Catalog está fora
- **WHEN** a prontidão é consultada
- **THEN** ele indica que está pronto
