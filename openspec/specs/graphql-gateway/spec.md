# graphql-gateway Specification

## Purpose

Ponto de entrada GraphQL único da loja: reúne as consultas das APIs migradas e as protege de consultas abusivas.

## Requirements

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

### Requirement: Cliente identificado no ponto de entrada
O ponto de entrada DEVE identificar o cliente pela credencial da sessão e repassá-la às APIs. Credencial inválida ou expirada DEVE ser recusada; consulta que exige login sem credencial, ou de cliente sem e-mail confirmado, DEVE receber erro de acesso, sem dados.

#### Scenario: Sem credencial
- **GIVEN** não há cliente identificado
- **WHEN** alguém consulta o cadastro pelo ponto de entrada
- **THEN** a resposta traz erro de acesso não autenticado e nenhum dado do cadastro

#### Scenario: E-mail não confirmado
- **GIVEN** o cliente está autenticado com o e-mail não confirmado
- **WHEN** ele consulta o cadastro
- **THEN** a resposta traz erro de acesso não autorizado

#### Scenario: Credencial expirada
- **GIVEN** a credencial da sessão expirou
- **WHEN** a loja chama o ponto de entrada com ela
- **THEN** a chamada é recusada como não autenticada e a loja pede o login de novo

#### Scenario: Catálogo sem login
- **GIVEN** não há cliente identificado
- **WHEN** a loja consulta os produtos
- **THEN** os produtos são devolvidos normalmente

### Requirement: Cadastro pelo ponto de entrada
A loja DEVE ler e salvar o cadastro do cliente pelo ponto de entrada único, com as mesmas regras e mensagens de hoje. Os erros de validação DEVEM vir por campo, sem nada salvo.

#### Scenario: Salvar com CPF inválido
- **GIVEN** o cliente está identificado
- **WHEN** salva o cadastro com o CPF 111.111.111-11
- **THEN** a resposta traz o erro no campo CPF e o cadastro não muda

#### Scenario: Meu cadastro pelo ponto de entrada
- **GIVEN** o cliente tem cadastro salvo
- **WHEN** abre "Meu cadastro" ou o checkout
- **THEN** vê os mesmos dados de antes, sem chamada direta à API de cadastro

### Requirement: Pedidos pelo ponto de entrada
A loja DEVE registrar pedidos, pagar de novo um pedido recusado e consultar os próprios pedidos pelo ponto de entrada único, com as mesmas regras de hoje. O acompanhamento do pagamento NÃO DEVE mudar.

#### Scenario: Compra pelo ponto de entrada
- **GIVEN** o cliente está identificado e o carrinho tem itens
- **WHEN** confirma o checkout
- **THEN** o pedido é registrado pelo ponto de entrada, a confirmação acompanha o pagamento como antes e o navegador não chama a API de pedidos diretamente

#### Scenario: Meus pedidos pelo ponto de entrada
- **GIVEN** o cliente tem pedidos
- **WHEN** abre "Meus pedidos" e um pedido
- **THEN** vê a mesma lista e o mesmo detalhe de antes

### Requirement: Erros do pedido no ponto de entrada
Os desfechos que não registram ou não alteram o pedido DEVEM chegar à loja como erro com código e a mensagem de hoje: pedido inválido, catálogo indisponível, pedido não encontrado e pagamento que não pode ser refeito.

#### Scenario: Pedido inválido
- **GIVEN** o cliente está identificado
- **WHEN** registra um pedido com CPF inválido
- **THEN** a resposta traz o código de validação com a mensagem "CPF inválido." e nada é registrado

#### Scenario: Catálogo indisponível
- **GIVEN** o Catalog está fora do ar
- **WHEN** o cliente registra um pedido
- **THEN** a resposta traz o código de indisponibilidade, nada é registrado nem cobrado, e a loja mantém o carrinho

#### Scenario: Pedido de outro cliente
- **GIVEN** o pedido A999 é de outro cliente
- **WHEN** o cliente consulta A999 ou tenta pagá-lo de novo
- **THEN** a resposta é "pedido não encontrado", como para um número inexistente
