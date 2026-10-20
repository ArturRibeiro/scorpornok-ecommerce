# Pedidos

A parte da loja que registra o pedido, pede a cobrança, acompanha o resultado e avisa o cliente na tela e por e-mail. Cada item indica a sua situação.

## 2. Pedido parado em "processando"

**Problema:** se o resultado do pagamento se perdia no caminho, o pedido ficava "em processamento" para sempre. O cliente não recebia resposta nem e-mail, e em alguns casos já tinha sido cobrado.

**Solução:**
- Depois de 15 minutos sem resposta, a loja pergunta à área de pagamentos o que aconteceu (veja [Pagamentos](payments.md)) e dá ao pedido uma situação definitiva: **confirmado**, **recusado** ou o novo **pagamento não processado**.
- "Pagamento não processado" é diferente de "recusado": significa que houve um problema da loja, que nada foi cobrado e que o cliente pode refazer a compra. O cliente vê isso na tela e recebe por e-mail.
- A equipe é avisada quando há pedidos parados além do esperado.

**Situação:** em funcionamento.

Detalhes técnicos: `handle-stuck-payments` (arquivada em `openspec/changes/archive/`).

## 3. Insistência diante de falhas curtas

**Problema:** se o sistema de pedidos ficava indisponível por alguns segundos na hora em que o resultado do pagamento chegava, o resultado ia para uma fila de problemas, e o pedido não era confirmado mesmo com o cliente cobrado.

**Solução:** a loja insiste por cerca de cinco minutos, com pausas crescentes, antes de desistir. Quedas curtas deixam de causar pedidos sem confirmação.

**Situação:** em funcionamento.

Detalhes técnicos: `add-messaging-retry-backoff` (arquivada em `openspec/changes/archive/`).

## 5. Tentar de novo com outro cartão

**Problema:** quando o cartão era recusado, o pedido terminava ali. O cliente precisava montar o carrinho e fazer um pedido novo.

**Solução:**
- O cliente pode tentar pagar **o mesmo pedido** com outro cartão, até três tentativas.
- Cada tentativa tem o seu resultado e o seu e-mail; um resultado atrasado de uma tentativa antiga não muda o pedido.
- Só o dono do pedido pode tentar de novo, e só depois de uma recusa do cartão. Um pedido com "pagamento não processado" (item 2) não tem nova tentativa: como o problema foi da loja, o cliente refaz a compra.
- O e-mail de recusa diz quantas tentativas ainda restam ou, na última, que elas acabaram.

**Situação:** em funcionamento.

Detalhes técnicos: `allow-payment-retry` (arquivada em `openspec/changes/archive/`).

## 6. E-mail que não chega

**Problema:** quando o e-mail com o resultado não podia ser entregue (por exemplo, endereço inexistente ou servidor de e-mail mal configurado), a loja tentava de novo a cada poucos segundos, para sempre. Um único e-mail com problema ainda travava os e-mails de todos os outros clientes. A equipe não ficava sabendo, e o cliente nunca era avisado.

**Solução:**
- A loja diferencia três situações:
  - **endereço do cliente inválido:** não adianta insistir; a loja desiste na hora e registra o motivo;
  - **problema passageiro com a mensagem:** tenta de novo até seis vezes, com pausas que dobram a cada vez, por cerca de meia hora; depois disso, desiste e registra o motivo;
  - **servidor de e-mail fora do ar ou mal configurado** (inclusive senha errada): espera o servidor voltar, verificando com pausas cada vez maiores, sem dar o e-mail de nenhum cliente como perdido. Quando ele volta, os e-mails pendentes saem.
- Um e-mail com problema não atrasa os e-mails dos outros clientes.
- A equipe é avisada quando o servidor de e-mail está fora ou quando e-mails deixam de ser entregues.

**Situação:** em funcionamento.

Detalhes técnicos: `limit-email-retries` (arquivada em `openspec/changes/archive/`).

## 7. Preço do pedido

**Problema:** a loja registrava o preço de cada produto como ele chegava do navegador. Quem alterasse o envio conseguia comprar pelo preço que quisesse.

**Solução:** no momento da compra, o preço, o nome e a foto de cada produto são conferidos com o catálogo. Se um produto não existir mais, o pedido é recusado com uma mensagem clara. Se o catálogo estiver fora do ar, a loja avisa que não conseguiu registrar o pedido e nada é cobrado.

**Situação:** em funcionamento.

Detalhes técnicos: `use-catalog-price-in-orders` (arquivada em `openspec/changes/archive/`).

## 8. Número do pedido que não se repete

**Problema:** o número do pedido era gerado de um jeito que não garantia que dois pedidos tivessem números diferentes. Como a cobrança única (item 1 em [Pagamentos](payments.md)) identifica cada cobrança pelo número do pedido, um pedido novo com o número de um antigo seria tratado como repetição: o cartão não seria cobrado, e o pedido podia ser confirmado com o resultado do pagamento do outro. Números repetidos também confundiriam a tela de confirmação e o e-mail.

**Solução:** cada pedido recebe um número sorteado, a letra A seguida de 12 dígitos (por exemplo, A483920175536). Há quase um trilhão de combinações, então a chance de repetir é desprezível. Mesmo assim, a loja recusa registrar um pedido com um número já usado: nesse caso raro, nada é gravado nem cobrado, o carrinho continua preenchido e o cliente pode confirmar de novo, recebendo outro número.

**Situação:** em funcionamento.

Detalhes técnicos: `unique-order-number` (arquivada em `openspec/changes/archive/`).

## 9. Dados de quem compra e endereço completo

**Problema:** o pedido não sabia quem estava comprando além do e-mail da conta, e o endereço de entrega era um texto livre (rua, cidade, estado, país), sem número, bairro ou conferência do CEP e do estado.

**Solução:**
- O pedido exige nome completo, telefone e CPF de quem compra, conferidos com as mesmas regras do cadastro. Um pedido com CPF ou telefone inválido é recusado com uma mensagem clara, sem registrar nem cobrar nada. Esses dados não ficam guardados no pedido: seguem só para o cadastro do cliente (veja [Cadastro do cliente](customers.md)).
- O endereço de entrega passa a ter rua, número, complemento (opcional), bairro, cidade, estado e CEP, sempre no Brasil, e acompanha o pedido. Ele pode ser diferente do endereço do cadastro.
- Ao registrar o pedido, a loja avisa o cadastro do cliente, junto com o registro do pedido: ou os dois acontecem, ou nenhum. Se o cadastro estiver fora do ar, o aviso espera, e o pedido segue normalmente.

**Situação:** em funcionamento.

Detalhes técnicos: `add-customer-profile` (arquivada em `openspec/changes/archive/`).

## 10. Consulta dos pedidos do cliente

**Problema:** a loja registrava os pedidos, mas não deixava o cliente consultá-los. Depois da tela da confirmação, só restava o e-mail com o resultado, e um pedido recusado não podia mais ser pago de novo se o cliente tivesse fechado aquela tela.

**Solução:**
- A loja passa a responder ao cliente a lista dos seus pedidos, do mais recente para o mais antigo, aos poucos, e o detalhe de cada um.
- O pedido aparece como foi comprado: os produtos, os preços e o endereço gravados na compra, mesmo que o produto tenha mudado de preço ou saído do catálogo, ou que o cadastro tenha outro endereço.
- Só o dono vê o pedido. Um pedido de outro cliente recebe a mesma resposta de um número que não existe ("pedido não encontrado"), sem revelar que o número existe.
- A consulta não devolve CPF, telefone, dados do cartão nem detalhes internos do pagamento ou do envio do e-mail.
- O detalhe diz se o cliente ainda pode pagar de novo, com a mesma regra das tentativas (item 5).

**Situação:** em funcionamento.

Detalhes técnicos: `add-order-history` (arquivada em `openspec/changes/archive/`).

## 11. Pedidos pelo ponto de entrada único

**Problema:** o checkout, a nova tentativa com outro cartão e "Meus pedidos" chamavam o serviço de pedidos direto, enquanto o catálogo e o cadastro já passavam pelo ponto de entrada único.

**Solução:**
- Registrar o pedido, pagar de novo um pedido recusado e consultar os próprios pedidos passam pelo ponto de entrada, com as mesmas regras de antes e a mesma identificação do cliente.
- Cada desfecho que não registra ou não altera o pedido chega à loja com um código e a mensagem de antes: pedido inválido (ex.: "CPF inválido."), catálogo fora do ar (nada é registrado nem cobrado), pedido não encontrado (também para pedido de outro cliente, sem revelar que o número existe) e pagamento que não pode ser refeito.
- O acompanhamento do pagamento na tela continua pelo canal de antes, sem consultas repetidas ao pedido.

**Situação:** em funcionamento.

Detalhes técnicos: `add-orders-subgraph` (arquivada em `openspec/changes/archive/`).
