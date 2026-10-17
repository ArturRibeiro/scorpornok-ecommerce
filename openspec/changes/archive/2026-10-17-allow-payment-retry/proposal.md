# Proposal

## Why

Hoje a loja esvazia o carrinho assim que o pedido é registrado, antes do resultado do pagamento, e um pedido recusado é definitivo. Depois de uma recusa, o cliente perde o carrinho e precisa montar tudo de novo para tentar outro cartão.

## What Changes

- **Front-end**:
  - O carrinho só é esvaziado quando o pagamento é aprovado ou fica pendente (sem resposta em 10 s); na recusa, ele é mantido.
  - A tela de recusa ganha "Tentar com outro cartão": o cliente informa outro cartão e acompanha o novo resultado na mesma tela.
- **Orders**:
  - Novo endpoint para pagar de novo um pedido recusado, só pelo dono.
  - O pedido volta a pendente e uma nova solicitação de pagamento sai com o número da tentativa.
  - Limite de 3 tentativas por pedido.
  - Só o resultado da tentativa atual muda o pedido; o de uma tentativa anterior, atrasado, é ignorado.
  - Cada tentativa resolvida gera o seu e-mail.
- **Payments**: um pagamento por tentativa do mesmo pedido, em vez de um por pedido.
- **Contratos**: `PaymentRequested`, `PaymentApproved` e `PaymentRejected` passam a levar o número da tentativa.
- **Docs**: UC03 e UC04 ganham o fluxo de nova tentativa; o `CLAUDE.md` descreve o endpoint e as tentativas.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `checkout`: carrinho mantido na recusa e nova tentativa com outro cartão.
- `order-placement`: nova tentativa de pagamento, limite e resultado só da tentativa atual.
- `payment-processing`: deduplicação por pedido e tentativa.
- `order-notification`: um e-mail por resultado de cada tentativa.

## Impact

- **Orders**: agregado `Order` e `OrderPayment` (tentativa atual), endpoint novo, consumidores e dispatcher de e-mail; recriar o volume do `orders-db` (sem migrations).
- **Payments**: chave de deduplicação passa a ser pedido + tentativa; recriar o volume do `payment-db`.
- **Front-end**: `CheckoutPage`, `OrderConfirmation` e o cliente do Orders.
- **Dependências**: exige `add-payment-idempotency` (mais de um pagamento por pedido, chave pedido + tentativa) e `tokenize-card-data` (o novo cartão chega como token, sem CVV). Implementar e arquivar depois das duas.

## Fora do escopo

- Trocar o endereço ou os itens de um pedido recusado.
- Nova tentativa por link no e-mail, fora da tela da confirmação.
- Retomar a nova tentativa depois de fechar a página (não há histórico de pedidos na loja).
- Cancelar um pedido recusado.
