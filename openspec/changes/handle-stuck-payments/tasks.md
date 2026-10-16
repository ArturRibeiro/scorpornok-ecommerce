# Tasks

## 1. Contratos e Payments

- [x] 1.1 Criar em `Shared.Code/IntegrationEvents` as mensagens `PaymentStatusRequested`, `PaymentStatusPending` e `PaymentNotProcessed`; verificar com `dotnet build Scorponok.slnx`
- [x] 1.2 No Payments, situação `Voided` no `Payment` e o handler da consulta: pagamento registrado → republica o resultado; em processamento → `PaymentStatusPending`; ausente → grava `Voided` e publica `PaymentNotProcessed`, relendo o registro em chave duplicada
- [x] 1.3 `ProcessPaymentHandler`: pagamento `Voided` → publica `PaymentRejected` com motivo de pedido anulado, sem chamar o gateway
- [x] 1.4 Consumidor `PaymentStatusRequestedConsumer` registrado no `AddInfrastructure`; testes do handler (as três respostas, solicitação depois da anulação sem gateway, corrida resolvida pela chave duplicada) e do consumidor no test harness; verificar com `dotnet test src/backend/Payments/Payments.Tests`

## 2. Domínio e verificação no Orders

- [x] 2.1 `OrderStatus.NotProcessed`, `Order.MarkPaymentNotProcessed()` (só sobre `Pending`), `OrderPayment.StatusCheckedAt` e `IsAwaitingPaymentEmail` incluindo `NotProcessed`; testes de domínio em `Orders.Tests`
- [x] 2.2 `PendingOrderWatcher` com as opções `PaymentCheck` (`TimeoutMinutes`, `IntervalSeconds`, `AlertAfterMinutes`), consulta no repositório e publicação pelo outbox na mesma transação que grava `StatusCheckedAt`; testes com repositório e publicador mockados (prazo não atingido, consulta publicada, nova consulta só depois do intervalo)
- [x] 2.3 Consumidores de `PaymentNotProcessed` (aplica o desfecho e avisa a loja) e `PaymentStatusPending` (só log); testes no test harness

## 3. Aviso ao cliente

- [x] 3.1 Texto de "pagamento não processado" no `OrderPaymentEmail`; teste do e-mail
- [x] 3.2 `status` em `PaymentStatusChanged` no hub e no notificador (mantendo `approved`); teste do `OrderHub` para pedido `NotProcessed`
- [x] 3.3 Front-end: `watchOrderPayment` devolve o status e a `OrderConfirmation` mostra "Pagamento não processado"; verificar com `npm run build && npm run lint`

## 4. Verificação operacional

- [x] 4.1 Check das filas de erro pela API de gerenciamento (`RabbitMq:ManagementUrl`), com a tag `ops`, no Orders e no Payments; `/health/ops` mapeado fora do `ready`; testes com `HttpMessageHandler` falso (vazia, com mensagens, API fora → `Degraded`)
- [x] 4.2 Check de pedidos presos no Orders; teste com repositório mockado
- [x] 4.3 Configuração nos `appsettings.Development.json` e no `docker-compose.yml` (`RabbitMq__ManagementUrl: http://rabbitmq:15672`, `PaymentCheck__*`)

## 5. Verificação ponta a ponta (compose)

- [x] 5.1 Payments parado: criar pedido com `PaymentCheck__TimeoutMinutes` baixo; o pedido continua `Pending` e `/health/ops` do Orders acusa pedido preso; subir o Payments e o pedido termina `Confirmed`
  - Verificado no compose (prazo 1 min, intervalo 10 s, tolerância 1 min; pedido A900000000001 inserido no `orders-db` e `PaymentRequested` publicado com o `payment-api` parado): o pedido ficou `Pending`, foi consultado a cada intervalo e o `/health/ops` do Orders respondeu 503 (pedido preso) com o `/health/ready` em 200.
  - Ao subir o Payments, a consulta foi processada antes da solicitação (filas diferentes): a tentativa foi anulada (`Voided`), a solicitação recusada sem gateway e o pedido terminou `NotProcessed`, com o e-mail no Mailpit e nada cobrado. É o desfecho coerente previsto no design (decisão 2); `Confirmed` só ocorre se a solicitação for processada antes da consulta, o que não dá para forçar com o Payments parado além do prazo.
- [x] 5.2 Resultado perdido: com o banco do Orders parado, aprovar um pagamento até a mensagem ir para `payment-approved_error`; subir o banco; a verificação confirma o pedido e `/health/ops` acusa a fila de erro até ela ser esvaziada pelo runbook
  - Verificado no compose: pedido A900000000002 inserido e `PaymentRequested` publicado com o `orders-db` parado; o Payments aprovou (`Approved`) e o `PaymentApproved` foi para `payment-approved_error` depois das tentativas (~5 min). Com o banco de volta, a consulta fez o Payments republicar o resultado e o pedido ficou `Confirmed` com o mesmo `PaymentId`, sem mexer na fila de erro.
  - O `/health/ops` respondeu 503 com `{"error-queues": "Mensagens nas filas de erro: payment-approved_error=1"}` (escritor JSON adicionado) até o runbook (`mover-fila-de-erro.py`) devolver a mensagem à fila de origem; reprocessada, não alterou o pedido e o `/health/ops` voltou a 200.
- [x] 5.3 Solicitação perdida: apagar a mensagem da fila `payment-requested`; o pedido termina `NotProcessed`, o e-mail chega no Mailpit, e reenviar a solicitação depois não cobra
  - Verificado no compose: pedido A900000000003 inserido e `PaymentRequested` publicado com o `payment-api` parado; a fila `payment-requested` foi esvaziada pela API de gerenciamento. Com o Payments de volta, a consulta anulou a tentativa (`Voided`), o pedido ficou `NotProcessed` e o e-mail "Pagamento não processado" chegou no Mailpit. Reenviar a solicitação depois manteve um único pagamento `Voided`, sem gateway, sem mudar o pedido e sem outro e-mail.

## 6. Documentação

- [x] 6.1 Runbook `docs/runbooks/filas-de-erro.md` (causa pelo `MT-Fault-Message`, correção, mover de volta), `CLAUDE.md` (verificação do pedido pendente, `NotProcessed`, `/health/ops`, configurações) e os casos de uso `UC03`/`UC04`
- [x] 6.2 Rodar `dotnet test Scorponok.slnx` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas

## Workflow follow-up

- Aplicar depois de `add-payment-idempotency`, que cria o registro "em processamento" usado na decisão 2.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
