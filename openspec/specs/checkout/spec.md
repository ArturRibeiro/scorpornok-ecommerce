# checkout Specification

## Purpose

Permite ao cliente transformar o carrinho em um pedido. O cliente revisa os itens, informa entrega e pagamento, confirma a compra e vê o resultado.

## Requirements

### Requirement: Acesso ao checkout
A loja DEVE oferecer uma página de checkout acessível a partir do carrinho. A página só DEVE ser exibida quando o carrinho tem pelo menos um item.

#### Scenario: Ir do carrinho para o checkout
- **GIVEN** o carrinho tem pelo menos um item
- **WHEN** o cliente aciona "Finalizar compra" no carrinho
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
- **WHEN** ele aciona "Editar carrinho"
- **THEN** a página do carrinho é exibida com os mesmos itens

### Requirement: Endereço de entrega
O checkout DEVE coletar rua, número, complemento, bairro, cidade, UF e CEP, todos obrigatórios exceto o complemento e validados com as regras do cadastro, e NÃO DEVE enviar o pedido com algum deles vazio ou inválido. Quando o cliente tem cadastro, esses campos DEVEM abrir preenchidos com o endereço dele; o que o cliente mudar ali DEVE valer só para aquele pedido, sem alterar um cadastro existente.

#### Scenario: Campo de endereço vazio
- **GIVEN** o cliente preencheu tudo menos a cidade
- **WHEN** ele aciona "Confirmar pedido"
- **THEN** o pedido não é enviado
- **AND** o campo cidade é indicado como obrigatório

#### Scenario: CEP inválido
- **GIVEN** o cliente informou o CEP 1234-567
- **WHEN** ele aciona "Confirmar pedido"
- **THEN** o pedido não é enviado e o CEP é indicado como inválido

#### Scenario: Endereço do cadastro
- **GIVEN** o cadastro do cliente tem o endereço Rua das Flores, 100, Centro, São Paulo, SP, CEP 01000-000
- **WHEN** ele abre o checkout
- **THEN** rua, número, bairro, cidade, UF e CEP já vêm preenchidos com esse endereço

#### Scenario: Endereço alterado no checkout
- **GIVEN** o checkout abriu com o endereço do cadastro
- **WHEN** o cliente troca a cidade e confirma o pedido
- **THEN** o pedido é enviado com a cidade digitada
- **AND** o endereço do cadastro continua o mesmo

#### Scenario: Sem endereço ou cadastro indisponível
- **GIVEN** o cliente não tem endereço no cadastro, ou o serviço de cadastro está fora do ar
- **WHEN** ele abre o checkout
- **THEN** os campos de endereço vêm em branco e o checkout funciona normalmente

### Requirement: Dados do comprador
O checkout DEVE coletar nome completo, telefone e CPF, obrigatórios e validados com as mesmas regras do cadastro, e NÃO DEVE enviar o pedido com algum deles vazio ou inválido. Quando o cliente tem cadastro, esses campos DEVEM abrir preenchidos; o que o cliente mudar ali DEVE valer só para aquele pedido, sem alterar um cadastro existente.

#### Scenario: Dados do cadastro
- **GIVEN** o cadastro do cliente tem o nome Maria Souza, o telefone (21) 99999-8888 e o CPF 529.982.247-25
- **WHEN** ele abre o checkout
- **THEN** nome, telefone e CPF já vêm preenchidos com esses valores

#### Scenario: CPF inválido no checkout
- **GIVEN** o cliente informou o CPF 111.111.111-11
- **WHEN** ele aciona "Confirmar pedido"
- **THEN** o pedido não é enviado e o CPF é indicado como inválido

#### Scenario: Sem cadastro
- **GIVEN** o cliente não tem cadastro, ou o serviço de cadastro está fora do ar
- **WHEN** ele abre o checkout
- **THEN** nome, telefone e CPF vêm em branco e precisam ser preenchidos para enviar o pedido

### Requirement: Dados do cartão
O checkout DEVE coletar nome do titular, número do cartão, mês e ano de validade, CVV e quantidade de parcelas (padrão 1), e DEVE enviar o titular, o número, a validade e o CVV só ao serviço de tokenização. Ele NÃO DEVE enviar o pedido com o número fora de 13 a 19 dígitos, o CVV fora de 3 ou 4 dígitos ou a validade no passado.

#### Scenario: Cartão vencido
- **GIVEN** a data atual é outubro de 2026
- **WHEN** o cliente informa a validade 09/2026 e aciona "Confirmar pedido"
- **THEN** o pedido não é enviado
- **AND** a validade é indicada como inválida

#### Scenario: Parcelas padrão
- **GIVEN** o cliente não escolheu a quantidade de parcelas
- **WHEN** o pedido é enviado
- **THEN** o pedido é enviado com 1 parcela

#### Scenario: Cartão só na tokenização
- **GIVEN** o cliente preencheu o cartão 4111111111111111 com CVV 123
- **WHEN** ele confirma o pedido
- **THEN** o número e o CVV são enviados só ao serviço de tokenização
- **AND** o pedido enviado ao serviço de pedidos leva o token, sem número nem CVV

### Requirement: Total exibido
O total exibido no carrinho e no checkout DEVE ser a soma de preço unitário vezes quantidade de cada item. Nenhuma das duas páginas DEVE exibir frete ou imposto.

#### Scenario: Total sem frete e imposto
- **GIVEN** o carrinho tem itens que somam $30.00
- **WHEN** o cliente vê o resumo no carrinho ou no checkout
- **THEN** o total exibido é $30.00
- **AND** não aparecem linhas de frete ou imposto

### Requirement: Envio do pedido
Ao confirmar, o checkout DEVE tokenizar o cartão e, em seguida, enviar ao serviço de pedidos os itens, o endereço de entrega, os dados do comprador, o token do cartão e as parcelas, em nome do cliente identificado. Enquanto a tokenização e o envio estão em andamento, o botão de confirmação DEVE ficar desabilitado.

#### Scenario: Envio em andamento
- **GIVEN** o formulário está válido
- **WHEN** o cliente aciona "Confirmar pedido"
- **THEN** o botão fica desabilitado e indica o envio até a resposta chegar

#### Scenario: E-mail no pedido
- **GIVEN** o cliente está identificado com cliente@exemplo.com
- **WHEN** ele confirma um pedido válido
- **THEN** o pedido é registrado na conta dele, com o e-mail cliente@exemplo.com

### Requirement: Pedido registrado
Quando o serviço de pedidos aceita o pedido, o checkout DEVE mostrar o número do pedido e indicar que o pagamento está em processamento. O carrinho DEVE ser esvaziado só quando o pagamento for aprovado ou ficar pendente, e mantido quando for recusado.

#### Scenario: Pedido aceito
- **GIVEN** o formulário está válido e o serviço de pedidos está no ar
- **WHEN** o cliente confirma o pedido e o serviço o aceita
- **THEN** é exibida uma confirmação com o número do pedido e a indicação de que o pagamento está em processamento
- **AND** o carrinho continua com os itens

#### Scenario: Carrinho após o resultado
- **GIVEN** o pedido foi aceito e o carrinho tem 2 itens
- **WHEN** o pagamento é aprovado, ou passam 10 segundos sem resultado
- **THEN** o carrinho fica vazio, inclusive no contador do cabeçalho

#### Scenario: Carrinho na recusa
- **GIVEN** o pedido foi aceito e o carrinho tem 2 itens
- **WHEN** o pagamento é recusado
- **THEN** o carrinho continua com os 2 itens

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

### Requirement: Nova tentativa com outro cartão
Quando o pagamento for recusado, o checkout DEVE oferecer pagar o mesmo pedido com outro cartão, sem refazer o carrinho, e voltar a acompanhar o resultado. Esgotadas as tentativas, NÃO DEVE oferecer outra.

#### Scenario: Pagar com outro cartão
- **GIVEN** o pagamento do pedido A123 foi recusado
- **WHEN** o cliente informa outro cartão e confirma
- **THEN** a confirmação volta a indicar pagamento em processamento para A123

#### Scenario: Tentativas esgotadas
- **GIVEN** o pedido A123 teve 3 pagamentos recusados
- **WHEN** chega a terceira recusa
- **THEN** a confirmação mostra a recusa sem a opção de tentar outro cartão
- **AND** o carrinho continua com os itens, para um novo pedido

### Requirement: Login exigido no checkout
O checkout DEVE exigir um cliente identificado. Sem sessão, o cliente DEVE ser levado ao login, com a opção de se cadastrar, e voltar ao checkout com o carrinho como estava.

#### Scenario: Checkout sem login
- **GIVEN** o cliente não está identificado e tem 2 itens no carrinho
- **WHEN** ele aciona "Finalizar compra" e entra
- **THEN** ele volta ao checkout com os mesmos 2 itens

#### Scenario: Sessão expirada ao confirmar
- **GIVEN** a sessão do cliente expirou sem poder ser renovada
- **WHEN** ele aciona "Confirmar pedido"
- **THEN** o pedido não é registrado e ele é levado ao login, mantendo o carrinho

### Requirement: E-mail da conta no checkout
O checkout DEVE exibir o e-mail da conta como destino do resultado do pagamento, sem campo para digitar outro.

#### Scenario: E-mail exibido
- **GIVEN** o cliente está identificado com cliente@exemplo.com
- **WHEN** ele abre o checkout
- **THEN** cliente@exemplo.com aparece como destino do resultado, sem campo de e-mail

### Requirement: Falha na tokenização do cartão
Quando o serviço de tokenização recusa o cartão ou não responde, o checkout NÃO DEVE criar o pedido e DEVE mostrar o motivo, mantendo o carrinho e os dados digitados.

#### Scenario: Tokenização indisponível
- **GIVEN** o serviço de tokenização está fora do ar
- **WHEN** o cliente aciona "Confirmar pedido" com o formulário válido
- **THEN** nenhum pedido é criado e é exibida uma mensagem de que não foi possível processar o cartão
- **AND** o carrinho e os campos preenchidos continuam como estavam
