# Spec Delta

## Purpose

Histórico de compras do cliente logado: a lista dos seus pedidos e o detalhe de cada um, como foram registrados na compra, com a situação do pagamento e o caminho para pagar de novo um pedido recusado.

## ADDED Requirements

### Requirement: Acesso a Meus pedidos
A loja DEVE oferecer a página "Meus pedidos" no menu da conta, ao lado de "Meu cadastro", e um link "Ver meus pedidos" na confirmação da compra. Só um cliente identificado, com e-mail confirmado, DEVE ver pedidos; sem sessão, a loja DEVE pedir o login.

#### Scenario: Pelo menu da conta
- **GIVEN** o cliente está identificado
- **WHEN** ele aciona "Meus pedidos" no menu da conta
- **THEN** a lista dos seus pedidos é exibida

#### Scenario: Pela confirmação da compra
- **GIVEN** o cliente acabou de registrar o pedido A123 e vê a confirmação
- **WHEN** ele aciona "Ver meus pedidos"
- **THEN** a lista dos seus pedidos é exibida, com A123 entre eles

#### Scenario: Sem sessão
- **GIVEN** não há cliente identificado
- **WHEN** alguém abre o endereço de "Meus pedidos" ou de um pedido
- **THEN** é pedido o login e nenhum pedido é exibido

#### Scenario: E-mail não confirmado
- **GIVEN** o cliente está autenticado com o e-mail ainda não confirmado
- **WHEN** ele consulta os seus pedidos
- **THEN** o acesso é recusado e nenhum pedido é exibido

### Requirement: Lista dos pedidos
A lista DEVE mostrar só os pedidos do cliente identificado, do mais recente para o mais antigo, cada um com número, data, situação, valor total, quantidade de itens e a foto de um dos produtos.

#### Scenario: Pedidos do cliente
- **GIVEN** o cliente fez o pedido A1 ontem e o A2 hoje, e outro cliente fez o A3
- **WHEN** ele abre "Meus pedidos"
- **THEN** a lista mostra A2 e depois A1, com número, data, situação, total, quantidade de itens e foto
- **AND** A3 não aparece

### Requirement: Carregamento aos poucos
A lista DEVE exibir os pedidos em blocos e oferecer "Carregar mais" enquanto houver pedidos não exibidos, sem repetir um pedido já exibido.

#### Scenario: Mais pedidos que um bloco
- **GIVEN** o cliente tem 25 pedidos e um bloco tem 10
- **WHEN** ele abre "Meus pedidos" e aciona "Carregar mais" duas vezes
- **THEN** a lista termina com os 25 pedidos, cada um uma vez, e "Carregar mais" deixa de aparecer

### Requirement: Nenhuma compra
Sem pedidos, a página DEVE informar "Você ainda não fez nenhuma compra" e oferecer um caminho de volta para a loja.

#### Scenario: Cliente sem pedidos
- **GIVEN** o cliente nunca registrou um pedido
- **WHEN** ele abre "Meus pedidos"
- **THEN** a página mostra "Você ainda não fez nenhuma compra" e um link para a loja

### Requirement: Detalhe do pedido
Ao abrir um pedido, a loja DEVE mostrar cada item com nome, foto, preço unitário, desconto, quantidade e subtotal, além do total, do endereço de entrega e da situação do pagamento.

#### Scenario: Abrir um pedido
- **GIVEN** o pedido A123 tem 2 unidades de um produto a R$ 100,00 e entrega na Rua A, 10, São Paulo/SP
- **WHEN** o cliente abre A123 na lista
- **THEN** o detalhe mostra o item com preço unitário R$ 100,00, quantidade 2 e subtotal R$ 200,00, o total R$ 200,00 e o endereço de entrega

### Requirement: Pedido como foi comprado
O histórico DEVE mostrar produtos, preços, descontos, quantidades e endereço de entrega registrados na compra, sem refletir mudanças posteriores no catálogo ou no cadastro.

#### Scenario: Produto mudou de preço
- **GIVEN** o pedido A123 registrou um produto a R$ 100,00 e hoje ele custa R$ 150,00
- **WHEN** o cliente abre A123
- **THEN** o item aparece a R$ 100,00

#### Scenario: Produto saiu da loja
- **GIVEN** um produto do pedido A123 não existe mais no catálogo
- **WHEN** o cliente abre a lista ou A123
- **THEN** o pedido e o item aparecem normalmente, com o nome e a foto da compra

#### Scenario: Cadastro mudou de endereço
- **GIVEN** o pedido A123 foi entregue na Rua A e o cliente mudou o cadastro para a Rua B
- **WHEN** ele abre A123
- **THEN** o endereço de entrega exibido é a Rua A

### Requirement: Situação do pedido
A lista e o detalhe DEVEM mostrar a situação como "Aguardando pagamento" (pagamento em andamento), "Pago" (aprovado), "Pagamento recusado" ou "Pagamento não processado" (nada foi cobrado e o cliente é orientado a refazer a compra).

#### Scenario: Pedido aprovado
- **GIVEN** o pagamento do pedido A123 foi aprovado
- **WHEN** o cliente vê A123 na lista ou no detalhe
- **THEN** a situação é "Pago"

#### Scenario: Pagamento não processado
- **GIVEN** o pagamento do pedido A123 não foi processado
- **WHEN** o cliente abre A123
- **THEN** a situação é "Pagamento não processado", com a informação de que nada foi cobrado e a orientação de refazer a compra

### Requirement: Detalhe acompanha o pagamento
O detalhe de um pedido "Aguardando pagamento" DEVE mudar para a nova situação quando o resultado do pagamento chegar, sem o cliente recarregar a página e sem consulta periódica ao pedido.

#### Scenario: Resultado chega com o detalhe aberto
- **GIVEN** o cliente está no detalhe do pedido A123, "Aguardando pagamento"
- **WHEN** o pagamento de A123 é aprovado
- **THEN** o detalhe passa a mostrar "Pago" sem recarregar a página

### Requirement: Nova tentativa pelo pedido
No detalhe de um pedido com "Pagamento recusado", a loja DEVE mostrar quantas tentativas restam e, enquanto houver, permitir pagar de novo com outro cartão e acompanhar o novo resultado. Sem tentativas, DEVE informar que é preciso refazer a compra.

#### Scenario: Pagar de novo pelo pedido
- **GIVEN** o pedido A123 teve o pagamento recusado na tentativa 1 de 3
- **WHEN** o cliente abre A123, informa outro cartão e confirma
- **THEN** o detalhe mostra que restavam 2 tentativas e passa a "Aguardando pagamento"

#### Scenario: Tentativas esgotadas
- **GIVEN** o pedido A123 teve o pagamento recusado nas 3 tentativas
- **WHEN** o cliente abre A123
- **THEN** o detalhe não oferece pagar de novo e informa que é preciso refazer a compra

### Requirement: Pedido de outro cliente
Um pedido de outro cliente e um número que não existe DEVEM ter a mesma resposta, "pedido não encontrado", sem revelar se o número existe.

#### Scenario: Número de outro cliente
- **GIVEN** o pedido A999 é de outro cliente
- **WHEN** o cliente abre o endereço do pedido A999
- **THEN** a loja mostra "pedido não encontrado", como para um número inexistente

### Requirement: Sem dados sensíveis
O histórico NÃO DEVE exibir nem devolver CPF, telefone, dados do cartão ou o estado interno do pagamento e do envio do e-mail.

#### Scenario: Consulta de um pedido
- **GIVEN** o pedido A123 foi feito com CPF, telefone e cartão
- **WHEN** o cliente consulta a lista ou o detalhe de A123
- **THEN** nenhum desses dados, nem o identificador do pagamento ou o estado do envio do e-mail, aparece na tela ou na resposta do serviço

### Requirement: Falha na consulta
Quando o serviço de pedidos não responde, a página DEVE informar que não foi possível carregar os pedidos e oferecer tentar de novo.

#### Scenario: Serviço fora do ar
- **GIVEN** o serviço de pedidos está fora do ar
- **WHEN** o cliente abre "Meus pedidos"
- **THEN** a página mostra que não foi possível carregar os pedidos, com a opção de tentar de novo
