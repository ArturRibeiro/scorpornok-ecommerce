# Pedidos

A parte da loja que registra o pedido, pede a cobrança, acompanha o resultado e avisa o cliente na tela e por e-mail. As soluções abaixo são **propostas**, exceto a 7, que está **em funcionamento**. Visão geral em [README.md](README.md).

## 2. Pedido parado em "processando"

**Problema:** se o resultado do pagamento se perde no caminho, o pedido fica "em processamento" para sempre. O cliente não recebe resposta nem e-mail, e em alguns casos já foi cobrado.

**Solução:**
- Depois de 15 minutos sem resposta, a loja pergunta à área de pagamentos o que aconteceu (veja [Pagamentos](payments.md)) e dá ao pedido uma situação definitiva: **confirmado**, **recusado** ou o novo **pagamento não processado**.
- "Pagamento não processado" é diferente de "recusado": significa que houve um problema da loja, que nada foi cobrado e que o cliente pode refazer a compra. O cliente vê isso na tela e recebe por e-mail.
- A equipe é avisada quando há pedidos parados além do esperado, e há um passo a passo para resolver.

Detalhes técnicos: `handle-stuck-payments`.

## 3. Insistência diante de falhas curtas

**Problema:** se o sistema de pedidos fica indisponível por alguns segundos na hora em que o resultado do pagamento chega, o resultado vai para uma fila de problemas, e o pedido não é confirmado mesmo com o cliente cobrado.

**Solução:** a loja insiste por cerca de cinco minutos, com pausas crescentes, antes de desistir. Quedas curtas deixam de causar pedidos sem confirmação.

Detalhes técnicos: `add-messaging-retry-backoff`.

## 5. Tentar de novo com outro cartão

**Problema:** quando o cartão é recusado, o pedido termina ali. O cliente precisa montar o carrinho e fazer um pedido novo.

**Solução:**
- O cliente pode tentar pagar **o mesmo pedido** com outro cartão, até três tentativas.
- Cada tentativa tem o seu resultado e o seu e-mail; um resultado atrasado de uma tentativa antiga não muda o pedido.
- Só o dono do pedido pode tentar de novo, e só depois de uma recusa.
- O e-mail de recusa lembra que ainda é possível tentar outro cartão, enquanto houver tentativas.

Detalhes técnicos: `allow-payment-retry`.

## 6. E-mail que não chega

**Problema:** se o e-mail com o resultado não pode ser entregue (por exemplo, endereço inexistente ou servidor de e-mail mal configurado), a loja tenta de novo a cada poucos segundos, para sempre. A equipe não fica sabendo, e o cliente nunca é avisado.

**Solução:**
- A loja diferencia três situações:
  - **endereço do cliente inválido:** não adianta insistir; a loja desiste na hora e registra o motivo;
  - **problema passageiro com a mensagem:** tenta de novo algumas vezes, com pausas crescentes, por cerca de uma hora;
  - **servidor de e-mail fora do ar ou mal configurado:** espera o servidor voltar sem dar o e-mail de nenhum cliente como perdido.
- Um e-mail com problema não atrasa os e-mails dos outros clientes.
- A equipe é avisada quando o servidor de e-mail está fora ou quando e-mails deixam de ser entregues.

Detalhes técnicos: `limit-email-retries`.

## 7. Preço do pedido

**Problema:** a loja registra o preço de cada produto como ele chega do navegador. Quem alterar o envio consegue comprar pelo preço que quiser.

**Solução:** no momento da compra, o preço, o nome e a foto de cada produto são conferidos com o catálogo. Se um produto não existir mais, o pedido é recusado com uma mensagem clara. Se o catálogo estiver fora do ar, a loja avisa que não conseguiu registrar o pedido e nada é cobrado.

Detalhes técnicos: `use-catalog-price-in-orders`.
