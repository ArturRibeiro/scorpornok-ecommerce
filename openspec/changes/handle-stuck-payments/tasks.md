# Tasks

## 1. Contratos e Payments

- [ ] 1.1 Criar em `Shared.Code/IntegrationEvents` as mensagens `PaymentStatusRequested`, `PaymentStatusPending` e `PaymentNotProcessed`; verificar com `dotnet build Scorponok.slnx`
- [ ] 1.2 No Payments, situação `Voided` no `Payment` e o handler da consulta: pagamento registrado → republica o resultado; em processamento → `PaymentStatusPending`; ausente → grava `Voided` e publica `PaymentNotProcessed`, relendo o registro em chave duplicada
- [ ] 1.3 `ProcessPaymentHandler`: pagamento `Voided` → publica `PaymentRejected` com motivo de pedido anulado, sem chamar o gateway
- [ ] 1.4 Consumidor `PaymentStatusRequestedConsumer` registrado no `AddInfrastructure`; testes do handler (as três respostas, solicitação depois da anulação sem gateway, corrida resolvida pela chave duplicada) e do consumidor no test harness; verificar com `dotnet test src/backend/Payments/Payments.Tests`

## 2. Domínio e verificação no Orders

- [ ] 2.1 `OrderStatus.NotProcessed`, `Order.MarkPaymentNotProcessed()` (só sobre `Pending`), `OrderPayment.StatusCheckedAt` e `IsAwaitingPaymentEmail` incluindo `NotProcessed`; testes de domínio em `Orders.Tests`
- [ ] 2.2 `PendingOrderWatcher` com as opções `PaymentCheck` (`TimeoutMinutes`, `IntervalSeconds`, `AlertAfterMinutes`), consulta no repositório e publicação pelo outbox na mesma transação que grava `StatusCheckedAt`; testes com repositório e publicador mockados (prazo não atingido, consulta publicada, nova consulta só depois do intervalo)
- [ ] 2.3 Consumidores de `PaymentNotProcessed` (aplica o desfecho e avisa a loja) e `PaymentStatusPending` (só log); testes no test harness

## 3. Aviso ao cliente

- [ ] 3.1 Texto de "pagamento não processado" no `OrderPaymentEmail`; teste do e-mail
- [ ] 3.2 `status` em `PaymentStatusChanged` no hub e no notificador (mantendo `approved`); teste do `OrderHub` para pedido `NotProcessed`
- [ ] 3.3 Front-end: `watchOrderPayment` devolve o status e a `OrderConfirmation` mostra "Pagamento não processado"; verificar com `npm run build && npm run lint`

## 4. Verificação operacional

- [ ] 4.1 Check das filas de erro pela API de gerenciamento (`RabbitMq:ManagementUrl`), com a tag `ops`, no Orders e no Payments; `/health/ops` mapeado fora do `ready`; testes com `HttpMessageHandler` falso (vazia, com mensagens, API fora → `Degraded`)
- [ ] 4.2 Check de pedidos presos no Orders; teste com repositório mockado
- [ ] 4.3 Configuração nos `appsettings.Development.json` e no `docker-compose.yml` (`RabbitMq__ManagementUrl: http://rabbitmq:15672`, `PaymentCheck__*`)

## 5. Verificação ponta a ponta (compose)

- [ ] 5.1 Payments parado: criar pedido com `PaymentCheck__TimeoutMinutes` baixo; o pedido continua `Pending` e `/health/ops` do Orders acusa pedido preso; subir o Payments e o pedido termina `Confirmed`
- [ ] 5.2 Resultado perdido: com o banco do Orders parado, aprovar um pagamento até a mensagem ir para `payment-approved_error`; subir o banco; a verificação confirma o pedido e `/health/ops` acusa a fila de erro até ela ser esvaziada pelo runbook
- [ ] 5.3 Solicitação perdida: apagar a mensagem da fila `payment-requested`; o pedido termina `NotProcessed`, o e-mail chega no Mailpit, e reenviar a solicitação depois não cobra

## 6. Documentação

- [ ] 6.1 Runbook `docs/runbooks/filas-de-erro.md` (causa pelo `MT-Fault-Message`, correção, mover de volta), `CLAUDE.md` (verificação do pedido pendente, `NotProcessed`, `/health/ops`, configurações) e os casos de uso `UC03`/`UC04`
- [ ] 6.2 Rodar `dotnet test Scorponok.slnx` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas

## Workflow follow-up

- Aplicar depois de `add-payment-idempotency`, que cria o registro "em processamento" usado na decisão 2.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
