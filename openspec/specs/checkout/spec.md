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
Ao confirmar, o checkout DEVE enviar ao serviço de pedidos os itens, o endereço, os dados do cartão e o identificador do cliente. Enquanto o envio está em andamento, o botão de confirmação DEVE ficar desabilitado para impedir envios duplicados.

#### Scenario: Envio em andamento
- **GIVEN** o formulário está válido
- **WHEN** o cliente aciona "Place order"
- **THEN** o botão fica desabilitado e indica que o pedido está sendo enviado até a resposta chegar

### Requirement: Pedido registrado
Quando o serviço de pedidos aceita o pedido, o checkout DEVE mostrar o número e o status do pedido e DEVE esvaziar o carrinho.

#### Scenario: Pedido aceito
- **GIVEN** o formulário está válido e o serviço de pedidos está no ar
- **WHEN** o cliente confirma o pedido e o serviço o aceita
- **THEN** é exibida uma confirmação com o número do pedido e o status pendente
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
