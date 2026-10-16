# Pagamentos

A parte da loja que recebe o pedido de cobrança, fala com o meio de pagamento (a empresa que processa o cartão) e devolve o resultado (aprovado ou recusado). Cada item indica a sua situação. Visão geral em [README.md](README.md).

## 1. Cobrança em dobro

**Problema:** o sistema cobrava o cartão e só depois anotava que tinha cobrado. Se algo falhasse entre uma coisa e outra, o sistema não sabia que já tinha cobrado, tentava de novo e o cliente pagava duas vezes. Duas cópias da mesma solicitação chegando ao mesmo tempo também podiam cobrar duas vezes.

**Solução:** o sistema passa a anotar que vai cobrar **antes** de cobrar, e cada cobrança leva um identificador único do pedido. Se o sistema precisar repetir a operação, o meio de pagamento reconhece o identificador e devolve o resultado da primeira cobrança, sem cobrar de novo. Cada tentativa de pagamento é cobrada no máximo uma vez. O identificador usa o número do pedido, por isso depende de os números nunca se repetirem (item 8 em [Pedidos](orders.md)).

**Situação:** em funcionamento.

Detalhes técnicos: `add-payment-idempotency` (arquivada em `openspec/changes/archive/`).

## 2. Resposta quando o pedido fica parado

**Problema:** às vezes a loja deixava de receber o resultado de um pagamento e o pedido ficava "em processamento" para sempre. No pior caso, o cliente era cobrado e o pedido nunca era confirmado.

**Solução:** quando um pedido passa do prazo sem resposta, a área de pedidos pergunta à área de pagamentos o que aconteceu, e a área de pagamentos é quem decide:
- se o pagamento foi feito, o resultado é reenviado e o pedido é confirmado ou recusado normalmente;
- se ainda está sendo feito, a loja espera um pouco mais;
- se a cobrança nem começou, ela é cancelada e não pode mais acontecer. Assim o cliente nunca é cobrado por um pedido que a loja já deu como não processado.

Se a área de pagamentos ficar fora do ar por mais tempo que o prazo, os pedidos feitos nesse período tendem a terminar como "pagamento não processado": nada é cobrado e o cliente é avisado para refazer a compra.

A equipe também passa a ser avisada quando mensagens de pagamento ficam travadas, e há um passo a passo para destravá-las.

**Situação:** em funcionamento.

Detalhes técnicos: `handle-stuck-payments` (arquivada em `openspec/changes/archive/`).

## 3. Insistência diante de falhas curtas

**Problema:** se a área de pagamentos ficava indisponível por poucos segundos, a loja desistia de processar o pagamento e ele ficava parado.

**Solução:** a loja insiste por cerca de cinco minutos, com pausas cada vez maiores entre as tentativas. Quedas curtas se resolvem sozinhas, sem ninguém precisar intervir, e um pedido de cobrança não se perde mesmo que a área de pagamentos reinicie no meio das tentativas. Mensagens com dados inválidos, que nunca vão dar certo, não são repetidas.

**Situação:** em funcionamento.

Detalhes técnicos: `add-messaging-retry-backoff` (arquivada em `openspec/changes/archive/`).

## 4. Proteção dos dados do cartão

**Problema:** os dados completos do cartão, inclusive o código de segurança, passavam por vários sistemas internos e podiam ficar guardados neles. As regras de segurança de cartões proíbem guardar o código de segurança.

**Solução:** assim que o cliente digita o cartão, ele é trocado por um código descartável, como fazem os meios de pagamento reais. Só esse código circula pela loja. Ele vale por 24 horas e serve para uma única cobrança. O código de segurança e o número completo do cartão nunca são guardados. Se o código descartável estiver vencido ou já tiver sido usado, o pagamento é recusado com uma explicação clara, e o cliente é avisado. Mesmo que o mesmo código chegue em duas cobranças ao mesmo tempo, só uma é aceita.

**Situação:** em funcionamento.

Detalhes técnicos: `tokenize-card-data` (arquivada em `openspec/changes/archive/`).

## 5. Mais de uma tentativa no mesmo pedido

**Problema:** hoje cada pedido só pode ter um pagamento.

**Solução:** cada pedido passa a aceitar algumas tentativas de pagamento, uma por cartão, e cada tentativa tem a sua própria proteção contra cobrança em dobro (item 1). Veja também o item 5 em [Pedidos](orders.md).

**Situação:** proposta.

Detalhes técnicos: `allow-payment-retry`.
