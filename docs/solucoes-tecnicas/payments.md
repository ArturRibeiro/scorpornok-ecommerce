# Pagamentos

A parte da loja que recebe o pedido de cobrança, fala com o meio de pagamento (a empresa que processa o cartão) e devolve o resultado (aprovado ou recusado). Todas as soluções abaixo são **propostas**. Visão geral em [README.md](README.md).

## 1. Cobrança em dobro

**Problema:** hoje o sistema cobra o cartão e só depois anota que cobrou. Se algo falhar entre uma coisa e outra, o sistema não sabe que já cobrou, tenta de novo e o cliente paga duas vezes.

**Solução:** o sistema passa a anotar que vai cobrar **antes** de cobrar, e cada cobrança leva um identificador único do pedido. Se o sistema precisar repetir a operação, o meio de pagamento reconhece o identificador e devolve o resultado da primeira cobrança, sem cobrar de novo. Cada tentativa de pagamento é cobrada no máximo uma vez.

Detalhes técnicos: `add-payment-idempotency`.

## 2. Resposta quando o pedido fica parado

**Problema:** às vezes a loja deixa de receber o resultado de um pagamento e o pedido fica "em processamento" para sempre. No pior caso, o cliente foi cobrado e o pedido nunca foi confirmado.

**Solução:** quando um pedido passa do prazo sem resposta, a área de pedidos pergunta à área de pagamentos o que aconteceu, e a área de pagamentos é quem decide:
- se o pagamento foi feito, o resultado é reenviado e o pedido é confirmado ou recusado normalmente;
- se ainda está sendo feito, a loja espera um pouco mais;
- se a cobrança nem começou, ela é cancelada e não pode mais acontecer. Assim o cliente nunca é cobrado por um pedido que a loja já deu como não processado.

A equipe também passa a ser avisada quando mensagens de pagamento ficam travadas, e há um passo a passo para destravá-las.

Detalhes técnicos: `handle-stuck-payments`.

## 3. Insistência diante de falhas curtas

**Problema:** hoje, se a área de pagamentos fica indisponível por poucos segundos, a loja desiste de processar o pagamento e ele fica parado.

**Solução:** a loja passa a insistir por cerca de cinco minutos, com pausas cada vez maiores entre as tentativas. Quedas curtas se resolvem sozinhas, sem ninguém precisar intervir. Mensagens com dados inválidos, que nunca vão dar certo, não são repetidas.

Detalhes técnicos: `add-messaging-retry-backoff`.

## 4. Proteção dos dados do cartão

**Problema:** hoje os dados completos do cartão, inclusive o código de segurança, passam por vários sistemas internos e podem ficar guardados neles. As regras de segurança de cartões proíbem guardar o código de segurança.

**Solução:** assim que o cliente digita o cartão, ele é trocado por um código descartável, como fazem os meios de pagamento reais. Só esse código circula pela loja. Ele vale por 24 horas e serve para uma única cobrança. O código de segurança e o número completo do cartão nunca são guardados. Se o código descartável estiver vencido ou já tiver sido usado, o pagamento é recusado com uma explicação clara, e o cliente é avisado.

Detalhes técnicos: `tokenize-card-data`.

## 5. Mais de uma tentativa no mesmo pedido

**Problema:** hoje cada pedido só pode ter um pagamento.

**Solução:** cada pedido passa a aceitar algumas tentativas de pagamento, uma por cartão, e cada tentativa tem a sua própria proteção contra cobrança em dobro (item 1). Veja também o item 5 em [Pedidos](orders.md).

Detalhes técnicos: `allow-payment-retry`.
