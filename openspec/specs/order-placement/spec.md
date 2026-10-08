# order-placement Specification

## Purpose

Define o contrato da API de Orders para registrar um pedido: que dados ela aceita, como valida e o que responde a quem a chama, inclusive a loja rodando no navegador.

## Requirements

### Requirement: Registro do pedido
A API de Orders DEVE aceitar um pedido com identificador do cliente, e-mail do cliente, endereço de entrega, itens e dados do cartão. Cada item tem identificador do produto (numérico, o mesmo do Catalog), nome, imagem, preço unitário, desconto e quantidade. O pedido aceito DEVE ser persistido com status pendente e com o e-mail informado.

#### Scenario: Pedido válido
- **GIVEN** um pedido com cliente, e-mail válido, endereço completo, um item com preço e quantidade maiores que zero e dados do cartão
- **WHEN** o pedido é enviado à API de Orders
- **THEN** o pedido é persistido com status pendente e com o e-mail informado

#### Scenario: Identificador de produto do Catalog
- **GIVEN** um item cujo identificador de produto é o número 42, como no Catalog
- **WHEN** o pedido é enviado
- **THEN** o pedido é aceito e o item é persistido com o identificador 42

### Requirement: Cliente do pedido
O pedido persistido DEVE pertencer ao cliente informado no envio. A API NÃO DEVE gerar outro identificador de cliente.

#### Scenario: Cliente preservado
- **GIVEN** um pedido válido enviado com o identificador de cliente C
- **WHEN** o pedido é persistido
- **THEN** o cliente do pedido é C

### Requirement: Resposta de pedido registrado
Quando o pedido é registrado, a API DEVE responder com sucesso de criação (HTTP 201). O corpo DEVE trazer o número do pedido, o status e o total, que é a soma de preço unitário vezes quantidade dos itens.

#### Scenario: Corpo da resposta
- **GIVEN** um pedido válido com 2 unidades a $10.00 e 1 unidade a $5.00
- **WHEN** o pedido é registrado
- **THEN** a resposta é HTTP 201
- **AND** o corpo traz um número de pedido não vazio, o status pendente e o total 25.00

### Requirement: Rejeição de pedido inválido
A API DEVE validar o pedido antes de persistir. Um pedido inválido NÃO DEVE ser persistido, e a API DEVE responder HTTP 400 com a lista de mensagens de validação. O pedido é inválido sem e-mail ou com e-mail em formato inválido, sem endereço completo, sem itens, ou com item de preço ou quantidade menor ou igual a zero.

#### Scenario: Endereço incompleto
- **GIVEN** um pedido sem cidade no endereço
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o endereço
- **AND** nenhum pedido é persistido

#### Scenario: Pedido sem itens
- **GIVEN** um pedido com a lista de itens vazia
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre os itens
- **AND** nenhum pedido é persistido

#### Scenario: Quantidade zero
- **GIVEN** um pedido com um item de quantidade 0
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre a quantidade

#### Scenario: E-mail ausente ou inválido
- **GIVEN** um pedido sem e-mail ou com o e-mail "cliente@"
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o e-mail
- **AND** nenhum pedido é persistido

### Requirement: Pedido pendente sem pagamento
O registro de um pedido pendente NÃO DEVE exigir um identificador de pagamento, porque o pagamento acontece depois da criação.

#### Scenario: Pedido novo sem pagamento
- **GIVEN** um pedido válido, ainda sem pagamento processado
- **WHEN** o pedido é enviado
- **THEN** o pedido é registrado com status pendente e sem identificador de pagamento

### Requirement: Acesso a partir da loja
A API de Orders DEVE aceitar chamadas feitas pelo navegador a partir das origens configuradas da loja e DEVE recusar as demais origens.

#### Scenario: Origem da loja
- **GIVEN** a origem `http://localhost:3000` está configurada
- **WHEN** o navegador envia um pedido a partir dessa origem
- **THEN** a requisição é permitida, inclusive a verificação prévia (preflight) do navegador

#### Scenario: Origem não configurada
- **GIVEN** a origem `http://exemplo.com` não está configurada
- **WHEN** o navegador tenta enviar um pedido a partir dela
- **THEN** o navegador bloqueia a requisição, porque a API não libera essa origem

### Requirement: Solicitação de pagamento
Depois de persistir um pedido válido, a API de Orders DEVE solicitar o pagamento ao contexto Payments com o número do pedido, os dados do cartão recebidos e o total calculado pelo próprio pedido, ignorando qualquer valor de cobrança enviado pelo cliente. Um pedido inválido NÃO DEVE gerar solicitação.

#### Scenario: Pedido válido solicita pagamento
- **GIVEN** um pedido válido com 2 unidades a $10.00 e 1 unidade a $5.00, enviado com valor de cobrança 1.00 no cartão
- **WHEN** o pedido é registrado
- **THEN** o pagamento do pedido é solicitado no valor de 25.00

#### Scenario: Pedido inválido não solicita pagamento
- **GIVEN** um pedido sem itens
- **WHEN** o pedido é enviado
- **THEN** a resposta é HTTP 400 e nenhum pagamento é solicitado

#### Scenario: Payments fora do ar
- **GIVEN** o contexto Payments está parado
- **WHEN** um pedido válido é enviado
- **THEN** a resposta é HTTP 201 com o status pendente, como de costume

### Requirement: Resultado do pagamento no pedido
Ao receber o resultado do pagamento, a API de Orders DEVE atualizar o pedido pendente: aprovado → status confirmado e o identificador do pagamento guardado; recusado → status de falha no pagamento. Um resultado para um pedido que já não está pendente NÃO DEVE alterá-lo.

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido A123 está pendente
- **WHEN** chega o resultado "aprovado" com o identificador de pagamento P
- **THEN** o pedido A123 fica confirmado com o identificador de pagamento P

#### Scenario: Pagamento recusado
- **GIVEN** o pedido A123 está pendente
- **WHEN** chega o resultado "recusado"
- **THEN** o pedido A123 fica com falha no pagamento e sem identificador de pagamento

#### Scenario: Resultado repetido
- **GIVEN** o pedido A123 já está confirmado
- **WHEN** chega de novo um resultado para A123
- **THEN** o pedido continua confirmado, sem mudanças

### Requirement: Notificação do resultado à loja
Depois de atualizar o pedido com o resultado do pagamento, a API de Orders DEVE enviar à loja que acompanha aquele pedido uma mensagem com o número do pedido e a indicação de aprovado ou recusado. Quem começar a acompanhar um pedido que já não está pendente DEVE receber o resultado imediatamente. Só as origens configuradas da loja DEVEM poder acompanhar pedidos.

#### Scenario: Resultado enviado a quem acompanha
- **GIVEN** a loja acompanha o pedido A123, que está pendente
- **WHEN** o pagamento do pedido A123 é aprovado e o pedido é confirmado
- **THEN** a loja recebe uma mensagem com o número A123 indicando pagamento aprovado

#### Scenario: Acompanhamento iniciado depois do resultado
- **GIVEN** o pedido A123 já está com falha no pagamento
- **WHEN** a loja começa a acompanhar o pedido A123
- **THEN** a loja recebe imediatamente uma mensagem com o número A123 indicando pagamento recusado

#### Scenario: Origem não configurada
- **GIVEN** a origem `http://exemplo.com` não está configurada
- **WHEN** o navegador tenta acompanhar um pedido a partir dela
- **THEN** a conexão é recusada

### Requirement: Registro do pedido independente do broker
A API de Orders DEVE responder ao registro de um pedido válido em até 5 segundos mesmo com o broker de mensagens indisponível, com o mesmo contrato de sempre (HTTP 201 e pedido pendente). A solicitação de pagamento DEVE ser entregue ao contexto Payments assim que o broker voltar, sem novo envio do cliente.

#### Scenario: Broker indisponível no registro
- **GIVEN** o broker de mensagens está parado
- **WHEN** um pedido válido é enviado
- **THEN** a resposta é HTTP 201 com o status pendente em até 5 segundos

#### Scenario: Broker volta depois do registro
- **GIVEN** o pedido A123 foi registrado com o broker parado e está pendente
- **WHEN** o broker volta a funcionar
- **THEN** o pagamento do pedido A123 é solicitado e o pedido deixa de estar pendente

#### Scenario: API reiniciada antes de o broker voltar
- **GIVEN** o pedido A123 foi registrado com o broker parado
- **WHEN** a API de Orders é reiniciada e só depois o broker volta
- **THEN** o pagamento do pedido A123 ainda é solicitado

### Requirement: Pedido e solicitação de pagamento atômicos
Um pedido válido registrado DEVE ter a sua solicitação de pagamento registrada junto, de forma atômica: ou os dois ficam registrados, ou nenhum. Nenhum pedido registrado DEVE ficar sem solicitação de pagamento por falha de comunicação com o broker.

#### Scenario: Falha ao registrar
- **GIVEN** o banco de dados do Orders falha durante o registro de um pedido válido
- **WHEN** o pedido é enviado
- **THEN** a resposta é de erro, o pedido não fica registrado e nenhum pagamento é solicitado

#### Scenario: Pedido sem cartão
- **GIVEN** um pedido válido enviado sem dados de cartão
- **WHEN** o pedido é registrado
- **THEN** a resposta é HTTP 201 com o status pendente e nenhum pagamento é solicitado
