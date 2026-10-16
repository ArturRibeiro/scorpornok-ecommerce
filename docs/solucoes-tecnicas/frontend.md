# Loja (site)

O site onde o cliente navega, monta o carrinho, paga e acompanha o resultado. Cada item indica a sua situação. Visão geral em [README.md](README.md).

## 4. Proteção dos dados do cartão

**Problema:** hoje o site envia o número completo do cartão e o código de segurança junto com o pedido.

**Solução:** quando o cliente confirma a compra, o site primeiro troca o cartão por um código descartável e só então envia o pedido, apenas com esse código. Para o cliente, nada muda na tela. Se o cartão for inválido, o erro aparece no próprio campo do cartão, e o formulário continua preenchido.

**Situação:** proposta.

Detalhes técnicos: `tokenize-card-data`.

## 5. Carrinho e nova tentativa

**Problema:** hoje o carrinho é esvaziado assim que o pedido é registrado, antes de o pagamento ser aprovado. Se o cartão for recusado, o cliente perde o carrinho e não tem como tentar outro cartão.

**Solução:**
- O carrinho só é esvaziado quando o pagamento é **aprovado** ou fica **pendente**. No pendente, o pedido já existe e ainda pode ser aprovado, e manter o carrinho levaria o cliente a comprar duas vezes.
- Na **recusa**, o carrinho continua, e a tela oferece "Tentar com outro cartão" enquanto houver tentativas (até três por pedido). O resultado da nova tentativa aparece na mesma tela.

**Situação:** proposta.

Detalhes técnicos: `allow-payment-retry`.

## 2. Pedido não processado

**Problema:** quando o resultado do pagamento não chega, a tela diz que o resultado será enviado por e-mail, mas, se o pedido ficar parado, esse e-mail nunca chega.

**Solução:** quando a loja descobre que o pagamento não chegou a ser feito, a tela mostra "Pagamento não processado": nada foi cobrado e o cliente pode refazer a compra. O cliente também recebe essa informação por e-mail.

**Situação:** proposta.

Detalhes técnicos: `handle-stuck-payments`.
