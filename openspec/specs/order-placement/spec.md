# order-placement Specification

## Purpose

Define o contrato da API de Orders para registrar um pedido: que dados ela aceita, como valida e o que responde a quem a chama, inclusive a loja rodando no navegador.

## Requirements

### Requirement: Registro do pedido
A API de Orders DEVE aceitar de um cliente autenticado um pedido com endereço, itens e dados do cartão. Cada item informa o identificador do produto (numérico, o mesmo do Catalog) e a quantidade. O pedido DEVE ser persistido pendente, com o e-mail da conta e o endereço de entrega informado (rua, número, complemento, bairro, cidade, UF e CEP, no Brasil).

#### Scenario: Pedido válido
- **GIVEN** o cliente autenticado cliente@exemplo.com envia um pedido com endereço completo, um item válido e cartão
- **WHEN** o pedido é registrado
- **THEN** ele é persistido pendente, com o e-mail cliente@exemplo.com e o endereço informado

#### Scenario: Identificador de produto do Catalog
- **GIVEN** um item com o identificador de produto 42, como no Catalog
- **WHEN** o pedido é registrado
- **THEN** o item é persistido com o identificador 42

### Requirement: Cliente do pedido
O pedido DEVE pertencer ao cliente autenticado que o enviou, ignorando qualquer identificador de cliente no corpo.

#### Scenario: Cliente preservado
- **GIVEN** o cliente autenticado C envia um pedido com o identificador de cliente D no corpo
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
A API DEVE validar o pedido antes de persistir e responder HTTP 400 com as mensagens de validação, sem persistir. O pedido é inválido sem endereço completo (rua, número, bairro, cidade, UF e CEP), com UF ou CEP inválidos, sem itens, com item de quantidade menor ou igual a zero, ou com item cujo produto não existe no Catalog.

#### Scenario: Endereço incompleto
- **GIVEN** um pedido sem cidade
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o endereço, e nada é persistido

#### Scenario: UF inválida
- **GIVEN** um pedido com a UF XX
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre a UF, e nada é persistido

#### Scenario: Pedido sem itens
- **GIVEN** um pedido sem itens
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre os itens, e nada é persistido

#### Scenario: Quantidade zero
- **GIVEN** um pedido com um item de quantidade 0
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre a quantidade

#### Scenario: Produto inexistente
- **GIVEN** o produto 999 não existe no Catalog
- **WHEN** um pedido com o produto 999 é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem citando o produto 999, e nada é persistido nem cobrado

#### Scenario: E-mail ausente ou inválido
- **GIVEN** o cliente autenticado cliente@exemplo.com envia um pedido válido sem e-mail ou com "cliente@" no corpo
- **WHEN** ele é enviado
- **THEN** o pedido é aceito com o e-mail cliente@exemplo.com

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

### Requirement: Resultado do pagamento no pedido
Ao receber o resultado do pagamento, a API de Orders DEVE atualizar o pedido pendente: aprovado → status confirmado e o identificador do pagamento guardado; recusado → status de falha no pagamento. Um resultado de outra tentativa que não a atual, ou para um pedido que já não está pendente, NÃO DEVE alterá-lo.

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

#### Scenario: Resultado depois de não processado
- **GIVEN** o pedido A123 está com pagamento não processado
- **WHEN** chega um resultado "recusado" para A123
- **THEN** o pedido continua com pagamento não processado

#### Scenario: Resultado atrasado de tentativa anterior
- **GIVEN** o pedido A123 está pendente na tentativa 2
- **WHEN** chega de novo o resultado "recusado" da tentativa 1
- **THEN** o pedido continua pendente na tentativa 2

### Requirement: Notificação do resultado à loja
Depois de atualizar o pedido com o resultado do pagamento, a API de Orders DEVE enviar o número do pedido e se foi aprovado a quem o acompanha; quem começar a acompanhar um pedido já resolvido DEVE receber o resultado na hora. Só o dono autenticado, a partir das origens configuradas da loja, DEVE poder acompanhar o pedido.

#### Scenario: Resultado enviado a quem acompanha
- **GIVEN** o dono acompanha o pedido pendente A123
- **WHEN** o pagamento de A123 é aprovado
- **THEN** a loja recebe A123 com pagamento aprovado

#### Scenario: Acompanhamento iniciado depois do resultado
- **GIVEN** o pedido A123 já está com falha no pagamento
- **WHEN** o dono começa a acompanhar A123
- **THEN** a loja recebe na hora A123 com pagamento recusado

#### Scenario: Origem não configurada
- **GIVEN** a origem `http://exemplo.com` não está configurada
- **WHEN** o navegador tenta acompanhar um pedido a partir dela
- **THEN** a conexão é recusada

#### Scenario: Sem credencial
- **GIVEN** a loja não envia credencial
- **WHEN** ela tenta acompanhar A123
- **THEN** a conexão é recusada

#### Scenario: Pedido de outro cliente
- **GIVEN** A123 pertence ao cliente C
- **WHEN** o cliente autenticado D tenta acompanhar A123
- **THEN** o acompanhamento é recusado, sem mensagem sobre A123

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

### Requirement: Prontidão independente do broker
A verificação de prontidão da API de Orders DEVE indicar que ela está pronta sempre que o seu banco de dados estiver disponível, mesmo com o broker de mensagens indisponível, porque o registro de pedidos não depende do broker. A API DEVE expor a disponibilidade do broker numa verificação separada, que indica falha enquanto o broker estiver indisponível.

#### Scenario: Broker indisponível
- **GIVEN** o banco do Orders está no ar e o broker de mensagens está parado
- **WHEN** a prontidão da API de Orders é consultada
- **THEN** a resposta indica que a API está pronta (HTTP 200)
- **AND** a verificação do broker responde com falha (HTTP 503)

#### Scenario: Tudo no ar
- **GIVEN** o banco do Orders e o broker de mensagens estão no ar
- **WHEN** a prontidão e a verificação do broker são consultadas
- **THEN** as duas respondem HTTP 200

#### Scenario: Banco indisponível
- **GIVEN** o banco do Orders está parado
- **WHEN** a prontidão da API de Orders é consultada
- **THEN** a resposta indica que a API não está pronta (HTTP 503)

### Requirement: Pedido exige autenticação
A API de Orders DEVE responder HTTP 401 a um pedido sem credencial ou com credencial inválida ou expirada, sem persistir o pedido nem solicitar pagamento.

#### Scenario: Sem credencial
- **GIVEN** um pedido válido sem credencial
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 401, e nada é persistido nem cobrado

#### Scenario: Credencial expirada
- **GIVEN** um pedido válido com credencial expirada
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 401

### Requirement: Pedido exige e-mail confirmado
A API de Orders DEVE responder HTTP 403 a um cliente autenticado cujo e-mail não foi confirmado, sem persistir o pedido.

#### Scenario: E-mail não confirmado
- **GIVEN** um cliente autenticado com e-mail não confirmado
- **WHEN** ele envia um pedido válido
- **THEN** a resposta é HTTP 403 e nada é persistido

### Requirement: Dados do item vindos do Catalog
A API de Orders DEVE registrar cada item com o preço unitário, o nome e a imagem do produto no Catalog no momento do registro, ignorando os valores desses campos enviados no pedido. O total do pedido e o valor cobrado DEVEM usar esse preço.

#### Scenario: Preço adulterado no pedido
- **GIVEN** o produto 42 custa $10.00 no Catalog
- **WHEN** o cliente envia um pedido com 2 unidades do produto 42 a $0.01
- **THEN** o item é registrado a $10.00, o total é 20.00 e o pagamento é solicitado no valor de 20.00

#### Scenario: Nome e imagem do Catalog
- **GIVEN** o produto 42 se chama "Camiseta" no Catalog
- **WHEN** o cliente envia um pedido com o produto 42 chamado "Brinde"
- **THEN** o item é registrado com o nome "Camiseta" e a imagem do Catalog

### Requirement: Catalog indisponível no registro
Quando o Catalog não puder ser consultado, a API de Orders DEVE responder HTTP 503, sem persistir o pedido nem solicitar pagamento.

#### Scenario: Catalog fora do ar
- **GIVEN** o Catalog está parado
- **WHEN** um pedido válido é enviado
- **THEN** a resposta é HTTP 503, e nada é persistido nem cobrado

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

### Requirement: Falha transitória ao aplicar o resultado
Um resultado de pagamento cuja aplicação ao pedido falhe por indisponibilidade temporária DEVE ser aplicado sem intervenção, desde que a falha dure no máximo 5 minutos. A falha NÃO DEVE perder o resultado, mesmo que a API de Orders seja reiniciada durante as tentativas.

#### Scenario: Banco do Orders volta em 2 minutos
- **GIVEN** o pedido A123 está pendente e o banco do Orders fica indisponível por 2 minutos
- **WHEN** chega o resultado "aprovado" de A123
- **THEN** A123 fica confirmado e o cliente é avisado, sem o resultado passar pela fila de erro

#### Scenario: Falha além do limite
- **GIVEN** o banco do Orders fica indisponível por 10 minutos
- **WHEN** chega o resultado do pagamento de A123
- **THEN** o resultado vai para a fila de erro depois de cerca de 5 minutos de tentativas

#### Scenario: Orders reiniciado durante as tentativas
- **GIVEN** o resultado de A123 está em novas tentativas
- **WHEN** a API de Orders é reiniciada
- **THEN** o resultado não se perde e é aplicado quando ela voltar

### Requirement: Verificação do pedido pendente
A API de Orders DEVE consultar o Payments sobre o pagamento de um pedido que continua pendente além do prazo configurado, e repetir a consulta a cada intervalo enquanto não houver resposta definitiva. Ela NÃO DEVE mudar o status do pedido sem a resposta do Payments.

#### Scenario: Prazo esgotado
- **GIVEN** o prazo é de 15 minutos e o pedido A123 está pendente há 16 minutos
- **WHEN** a verificação roda
- **THEN** o Payments é consultado sobre o pagamento de A123

#### Scenario: Payments sem resposta
- **GIVEN** o pedido A123 passou do prazo e o Payments está parado
- **WHEN** a verificação roda
- **THEN** o pedido A123 continua pendente e é consultado de novo no intervalo seguinte

### Requirement: Desfecho da verificação
A API de Orders DEVE aplicar ao pedido pendente a resposta da consulta: aprovado → confirmado; recusado → falha no pagamento; sem pagamento → pagamento não processado; em processamento → continua pendente.

#### Scenario: Pagamento aprovado e resultado perdido
- **GIVEN** o pagamento de A123 foi aprovado, mas o resultado não chegou ao Orders
- **WHEN** a consulta de A123 responde "aprovado"
- **THEN** o pedido A123 fica confirmado com o identificador do pagamento

#### Scenario: Solicitação nunca processada
- **GIVEN** a solicitação de pagamento de A123 nunca foi processada
- **WHEN** a consulta de A123 responde "sem pagamento"
- **THEN** o pedido A123 fica com pagamento não processado

#### Scenario: Pagamento em andamento
- **GIVEN** o pagamento de A123 está em processamento
- **WHEN** a consulta de A123 responde "em processamento"
- **THEN** o pedido A123 continua pendente

### Requirement: Pagamento não processado avisado à loja
Quem acompanha um pedido com pagamento não processado DEVE receber esse desfecho, distinto de "pagamento recusado".

#### Scenario: Acompanhamento de pedido não processado
- **GIVEN** o pedido A123 está com pagamento não processado
- **WHEN** o dono começa a acompanhar A123
- **THEN** a loja recebe na hora A123 com pagamento não processado, e não como recusado

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

### Requirement: Nova tentativa de pagamento
A API de Orders DEVE aceitar do dono de um pedido com falha no pagamento um novo cartão, voltar o pedido a pendente e solicitar de novo o pagamento do mesmo total. Cada pedido DEVE ter no máximo 3 tentativas.

#### Scenario: Nova tentativa aceita
- **GIVEN** o pedido A123, do cliente C, está com falha no pagamento após 1 tentativa
- **WHEN** C envia outro cartão para A123
- **THEN** a resposta é HTTP 202, A123 volta a pendente e o pagamento é solicitado como tentativa 2

#### Scenario: Pedido não recusado
- **GIVEN** o pedido A123 está pendente ou confirmado
- **WHEN** o dono envia outro cartão para A123
- **THEN** a resposta é HTTP 409, e nada muda nem é cobrado

#### Scenario: Limite de tentativas
- **GIVEN** o pedido A123 já teve 3 tentativas recusadas
- **WHEN** o dono envia outro cartão para A123
- **THEN** a resposta é HTTP 409, e nada é cobrado

#### Scenario: Pedido de outro cliente
- **GIVEN** o pedido A123 pertence ao cliente C
- **WHEN** o cliente D envia um cartão para A123
- **THEN** a resposta é HTTP 404, e nada muda

### Requirement: Dados do comprador no pedido
A API de Orders DEVE exigir no pedido o nome completo, o telefone e o CPF do comprador, validados com as mesmas regras do cadastro, e responder HTTP 400 sem persistir quando faltarem ou forem inválidos. Esses dados NÃO DEVEM ser guardados no pedido.

#### Scenario: CPF inválido no pedido
- **GIVEN** um pedido válido com o CPF 111.111.111-11
- **WHEN** ele é enviado
- **THEN** a resposta é HTTP 400 com uma mensagem sobre o CPF, e nada é persistido nem cobrado

### Requirement: Informações da compra para o cadastro
Um pedido registrado DEVE enviar ao serviço de cadastro o cliente, o e-mail, o nome, o telefone, o CPF, o número do pedido e o endereço de entrega do pedido, de forma atômica com o registro. O envio NÃO DEVE depender do broker ou do serviço de cadastro estarem no ar no momento do registro.

#### Scenario: Pedido registrado
- **GIVEN** o cliente envia o pedido A123 com entrega no Rio de Janeiro
- **WHEN** o pedido é registrado
- **THEN** o serviço de cadastro recebe as informações do pedido A123 com o endereço do Rio de Janeiro

#### Scenario: Broker indisponível
- **GIVEN** o broker de mensagens está parado
- **WHEN** o pedido A123 é registrado
- **THEN** a resposta é HTTP 201
- **AND** as informações do pedido A123 chegam ao serviço de cadastro quando o broker volta

#### Scenario: Falha ao registrar
- **GIVEN** o banco de dados do Orders falha durante o registro do pedido
- **WHEN** o pedido é enviado
- **THEN** nenhuma informação do pedido é enviada ao serviço de cadastro
