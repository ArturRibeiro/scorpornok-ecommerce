# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- `Order.ConfirmPayment`/`FailPayment` só agem sobre pedido `Pending`; `OrderPaymentHandlers` ignora resultado para pedido que já não está pendente ou que não existe.
- Os consumidores do Orders (`PaymentApproved`/`PaymentRejectedConsumer`) e o `PaymentRequestedConsumer` do Payments usam `UseMessageRetry(Interval(3, 1 s))`; esgotadas as tentativas, o MassTransit move a mensagem para `<fila>_error`, que ninguém lê.
- O `ProcessPaymentHandler` já não cobra de novo se existe `Payment` para o `OrderNumber` (índice único). A change `add-payment-idempotency` acrescenta o registro "em processamento" antes da cobrança e move o índice único para a chave `{OrderNumber}-{Attempt}`.
- O Orders não guarda o cartão: o `PaymentRequested` só existe no outbox até a entrega. Reenviar a solicitação a partir do Orders não é possível.
- O `OrderEmailDispatcher` só envia e-mail para `Confirmed`/`Failed` (`IsAwaitingPaymentEmail`).
- O hub envia `PaymentStatusChanged { orderNumber, approved }`; a loja mostra "Pendente" após 10 s sem mensagem.

## Goals / Non-Goals

**Goals:**
- O Payments é a fonte da verdade: o Orders nunca dá um desfecho sem a resposta dele.
- Nenhum cenário termina com o cliente cobrado e o pedido sem confirmação.
- A operação enxerga as filas de erro e os pedidos presos sem abrir o painel do RabbitMQ.

**Non-Goals:**
- Estorno, reprocessamento automático das filas de erro e mudança da política de retry.

## Decisions

### 1. Consultar o Payments em vez de expirar às cegas
Ao passar do prazo, o Orders publica `PaymentStatusRequested(OrderNumber)` (pelo outbox). O Payments responde:
- com um pagamento registrado: republica `PaymentApproved`/`PaymentRejected`, o caminho que já existe e que já é idempotente no Orders;
- com o pagamento "em processamento": publica `PaymentStatusPending(OrderNumber)`;
- sem pagamento: anula o pedido (decisão 2) e publica `PaymentNotProcessed(OrderNumber)`.

Isso resolve o resultado aprovado parado no `payment-approved_error` do Orders: a consulta faz o Payments republicar o resultado, sem tocar na fila de erro.

- *Alternativa:* expirar pelo prazo e estornar se a aprovação chegar depois: exige estorno no gateway e deixa o cliente cobrado e depois reembolsado.
- *Alternativa:* aceitar a aprovação tardia num pedido expirado: o cliente já recebeu "não processado" e pode ter refeito a compra; seriam duas cobranças.

### 2. Anulação no Payments, serializada pelo índice único
"Sem pagamento" grava um `Payment` com situação `Voided` para a tentativa atual, com a mesma chave de idempotência (`{OrderNumber}-{Attempt}`) e o mesmo índice único que `add-payment-idempotency` usa na cobrança. Por isso `PaymentStatusRequested` leva também o `Attempt` (1 até `allow-payment-retry`). Com o registro "em processamento" de `add-payment-idempotency`, a consulta e a cobrança disputam a mesma linha: a que gravar primeiro vence, e a outra encontra o registro e age conforme ele (a cobrança encontra `Voided` e publica `PaymentRejected` com motivo "pedido anulado", sem chamar o gateway). Uma `DbUpdateException` de chave duplicada é tratada relendo o registro.

### 3. Novo status `NotProcessed`
`OrderStatus.NotProcessed` (código 11, "Pagamento não processado"), com `Order.MarkPaymentNotProcessed()` que, como os outros, só age sobre `Pending`. Reutilizar `Failed` faria o e-mail e a loja dizerem "cartão recusado", o que é falso.

`IsAwaitingPaymentEmail` passa a incluir `NotProcessed`; o `OrderPaymentEmail` ganha o texto "não foi possível processar o pagamento; nada foi cobrado; refaça a compra".

### 4. Verificação em segundo plano no Orders
`PendingOrderWatcher` (`BackgroundService`, mesmo padrão do `OrderEmailDispatcher`): a cada `PaymentCheck:IntervalSeconds` busca pedidos `Pending` com `OrderDate` além de `PaymentCheck:TimeoutMinutes` (padrão 15) e `Payment.StatusCheckedAt` nulo ou mais antigo que o intervalo; publica a consulta e grava `StatusCheckedAt` na mesma transação (outbox). `PaymentStatusPending` só registra o log; o pedido é consultado de novo na rodada seguinte.

### 5. Mensagem do hub com o status
`PaymentStatusChanged` ganha `status` (`Confirmed`, `Failed`, `NotProcessed`); `approved` continua, para compatibilidade. A `OrderConfirmation` da loja mostra "Pagamento não processado" quando `status` for `NotProcessed`.

### 6. Verificação operacional `/health/ops`
Em cada API, um check com a tag `ops`, mapeado em `/health/ops` e fora do `ready`:
- **filas de erro**: consulta a API de gerenciamento do RabbitMQ (`GET /api/queues/{vhost}/{fila}`, configuração `RabbitMq:ManagementUrl`) para as `_error` das filas que a API consome; `Unhealthy` com mensagens, citando fila e quantidade;
- **pedidos presos** (só Orders): `Pending` além de `TimeoutMinutes + PaymentCheck:AlertAfterMinutes` → `Unhealthy` com a quantidade.

API de gerenciamento fora do ar → `Degraded` (não sabemos, mas não é falha do fluxo).

- *Alternativa:* consumir as filas `_error` com um consumidor que registra e alerta: tira as mensagens de onde a operação as reprocessa.

### 7. Reprocessamento documentado
Runbook em `docs/runbooks/filas-de-erro.md`: identificar a causa pelo cabeçalho `MT-Fault-Message` no painel, corrigir, e mover as mensagens de volta para a fila de origem (painel do RabbitMQ, plugin shovel). É seguro porque o Payments não cobra de novo e o Orders ignora resultado para pedido com desfecho.

## Risks / Trade-offs

- [Payments fora por muito tempo: o pedido fica pendente até ele voltar] → aparece em `/health/ops`; o cliente já foi avisado na tela de que o resultado vai por e-mail.
- [Consulta e solicitação chegam por filas diferentes e podem ser processadas em qualquer ordem] → decisão 2: o índice único decide, e as duas ordens levam a um desfecho coerente.
- [A verificação de filas depende da API de gerenciamento (porta 15672, credenciais)] → configuração separada, `Degraded` se indisponível.
- [`OrderDate` usa `DateTime.Now`] → o prazo é comparado no mesmo relógio do Orders; migrar para UTC fica fora do escopo.

## Migration Plan

- Recriar os volumes `orders_data` e `payment_data` (colunas novas, sem migrations).
- Subir o Payments antes do Orders, para a fila de consulta existir antes da primeira publicação.
- Rollback: reverter o commit; pedidos `NotProcessed` gravados exigiriam recriar o volume.
