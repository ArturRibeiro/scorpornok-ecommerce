# checkout Specification

## Purpose

Permite ao cliente transformar o carrinho em um pedido. O cliente revisa os itens, informa entrega e pagamento, confirma a compra e vê o resultado.

## Requirements

### Requirement: Acesso ao checkout
A loja DEVE oferecer uma página de checkout acessível a partir do carrinho. A página só DEVE ser exibida quando o carrinho tem pelo menos um item.

#### Scenario: Ir do carrinho para o checkout
- **GIVEN** o carrinho tem pelo menos um item
- **WHEN** o cliente aciona "Proceed to Checkout" no carrinho
- **THEN** a página de checkout é exibida, sem cair na página de "não encontrado"

#### Scenario: Checkout com o carrinho vazio
- **GIVEN** o carrinho está vazio
- **WHEN** o cliente abre o endereço do checkout
- **THEN** ele é levado para a página do carrinho

### Requirement: Revisão dos itens
O checkout DEVE listar, só para leitura, cada item do carrinho com imagem, nome, quantidade, preço unitário e subtotal da linha. DEVE também oferecer um caminho de volta ao carrinho para editar os itens.

#### Scenario: Itens exibidos
- **GIVEN** o carrinho tem 2 unidades de um produto de $10.00 e 1 unidade de um produto de $5.00
- **WHEN** o cliente abre o checkout
- **THEN** são exibidas duas linhas, com subtotais de $20.00 e $5.00
- **AND** as quantidades não podem ser alteradas nessa página

#### Scenario: Voltar para editar
- **GIVEN** o cliente está no checkout
- **WHEN** ele aciona "Edit cart"
- **THEN** a página do carrinho é exibida com os mesmos itens

### Requirement: Endereço de entrega
O checkout DEVE coletar rua, cidade, estado, país e CEP, todos obrigatórios. Ele NÃO DEVE enviar o pedido enquanto algum desses campos estiver vazio.

#### Scenario: Campo de endereço vazio
- **GIVEN** o cliente preencheu tudo menos a cidade
- **WHEN** ele aciona "Place order"
- **THEN** o pedido não é enviado
- **AND** o campo cidade é indicado como obrigatório

### Requirement: Dados do cartão
O checkout DEVE coletar nome do titular, número do cartão, mês e ano de validade, CVV e quantidade de parcelas (padrão 1). Ele NÃO DEVE enviar o pedido com o número fora de 13 a 19 dígitos, o CVV fora de 3 ou 4 dígitos ou a validade no passado.

#### Scenario: Cartão vencido
- **GIVEN** a data atual é outubro de 2026
- **WHEN** o cliente informa a validade 09/2026 e aciona "Place order"
- **THEN** o pedido não é enviado
- **AND** a validade é indicada como inválida

#### Scenario: Parcelas padrão
- **GIVEN** o cliente não escolheu a quantidade de parcelas
- **WHEN** o pedido é enviado
- **THEN** o pedido é enviado com 1 parcela

### Requirement: Total exibido
O total exibido no carrinho e no checkout DEVE ser a soma de preço unitário vezes quantidade de cada item. Nenhuma das duas páginas DEVE exibir frete ou imposto.

#### Scenario: Total sem frete e imposto
- **GIVEN** o carrinho tem itens que somam $30.00
- **WHEN** o cliente vê o resumo no carrinho ou no checkout
- **THEN** o total exibido é $30.00
- **AND** não aparecem linhas de frete ou imposto

### Requirement: Identificação anônima do cliente
O checkout DEVE enviar um identificador de cliente gerado no próprio navegador. O mesmo identificador DEVE ser reutilizado nos pedidos seguintes feitos naquele navegador.

#### Scenario: Dois pedidos no mesmo navegador
- **GIVEN** o cliente já fez um pedido neste navegador
- **WHEN** ele faz um segundo pedido
- **THEN** os dois pedidos são enviados com o mesmo identificador de cliente

### Requirement: Envio do pedido
Ao confirmar, o checkout DEVE enviar ao serviço de pedidos os itens, o endereço, os dados do cartão, o e-mail e o identificador do cliente. Enquanto o envio está em andamento, o botão de confirmação DEVE ficar desabilitado para impedir envios duplicados.

#### Scenario: Envio em andamento
- **GIVEN** o formulário está válido
- **WHEN** o cliente aciona "Place order"
- **THEN** o botão fica desabilitado e indica que o pedido está sendo enviado até a resposta chegar

#### Scenario: E-mail no pedido
- **GIVEN** o formulário está válido com o e-mail cliente@exemplo.com
- **WHEN** o cliente aciona "Place order"
- **THEN** o pedido enviado ao serviço de pedidos contém o e-mail cliente@exemplo.com

### Requirement: Pedido registrado
Quando o serviço de pedidos aceita o pedido, o checkout DEVE mostrar o número do pedido, DEVE indicar que o pagamento está em processamento e DEVE esvaziar o carrinho.

#### Scenario: Pedido aceito
- **GIVEN** o formulário está válido e o serviço de pedidos está no ar
- **WHEN** o cliente confirma o pedido e o serviço o aceita
- **THEN** é exibida uma confirmação com o número do pedido e a indicação de que o pagamento está em processamento
- **AND** o carrinho fica vazio, inclusive no contador do cabeçalho

### Requirement: Falha no envio
Quando o serviço de pedidos recusa o pedido ou não responde, o checkout DEVE mostrar o motivo e DEVE manter o carrinho e os dados digitados, para que o cliente tente de novo.

#### Scenario: Pedido recusado por validação
- **GIVEN** o serviço de pedidos responde com mensagens de validação
- **WHEN** o cliente confirma o pedido
- **THEN** as mensagens são exibidas no checkout
- **AND** o carrinho e os campos preenchidos continuam como estavam

#### Scenario: Serviço indisponível
- **GIVEN** o serviço de pedidos está fora do ar
- **WHEN** o cliente confirma o pedido
- **THEN** é exibida uma mensagem de que não foi possível enviar o pedido
- **AND** o carrinho e os campos preenchidos continuam como estavam

### Requirement: Acompanhamento do pagamento
Depois que o pedido é aceito, o checkout DEVE receber do serviço de pedidos a mensagem com o resultado do pagamento, sem consultar o pedido periodicamente. Se o pagamento for aprovado, DEVE mostrar que o pedido foi concluído com sucesso; se for recusado, DEVE mostrar que o pagamento foi recusado. Se a mensagem não chegar em até 10 segundos, ou se a conexão para recebê-la não puder ser aberta, o checkout DEVE informar que o pagamento está pendente e que o resultado será enviado para o e-mail do cliente.

#### Scenario: Pagamento aprovado
- **GIVEN** o pedido foi aceito e a confirmação indica pagamento em processamento
- **WHEN** chega a mensagem de que o pagamento do pedido foi aprovado
- **THEN** a confirmação mostra que o pedido foi concluído com sucesso
- **AND** o checkout não fez consultas periódicas ao pedido

#### Scenario: Pagamento recusado
- **GIVEN** o pedido foi aceito e a confirmação indica pagamento em processamento
- **WHEN** chega a mensagem de que o pagamento do pedido foi recusado
- **THEN** a confirmação mostra que o pagamento foi recusado

#### Scenario: Mensagem não chega
- **GIVEN** o pedido foi aceito com o e-mail cliente@exemplo.com e o pagamento não é processado
- **WHEN** passam 10 segundos sem a mensagem do resultado
- **THEN** o checkout informa que o pagamento está pendente e que o resultado será enviado para cliente@exemplo.com

### Requirement: E-mail do cliente
O checkout DEVE coletar o e-mail do cliente, obrigatório e em formato válido, e DEVE informar que o resultado do pagamento será enviado para ele. Ele NÃO DEVE enviar o pedido enquanto o e-mail estiver vazio ou inválido.

#### Scenario: E-mail vazio
- **GIVEN** o cliente preencheu tudo menos o e-mail
- **WHEN** ele aciona "Place order"
- **THEN** o pedido não é enviado
- **AND** o campo de e-mail é indicado como obrigatório

#### Scenario: E-mail inválido
- **GIVEN** o cliente digitou "cliente@" no e-mail
- **WHEN** ele aciona "Place order"
- **THEN** o pedido não é enviado
- **AND** o campo de e-mail é indicado como inválido
