# Tasks

## 1. Contratos e domínio do Orders

- [x] 1.1 Adicionar `int Attempt = 1` a `PaymentRequested`, `PaymentApproved` e `PaymentRejected`; verificar com `dotnet build Scorponok.slnx`
- [x] 1.2 `OrderPayment.Attempt`, `Order.RetryPayment()` (só `Failed`, limite 3, volta a `Pending`, zera `EmailSentAt`) e `ConfirmPayment`/`FailPayment` exigindo a tentativa atual
- [x] 1.3 Testes do `Order`: nova tentativa a partir de `Failed`, recusa de `Pending`/`Confirmed`, limite de 3, resultado de tentativa anterior ignorado, e-mail liberado para a nova tentativa; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 2. Fluxo no Orders

- [x] 2.1 `RetryOrderPaymentCommand` e handler: dono do pedido, `RetryPayment()`, `PaymentRequested` com o `Total` e a tentativa, tudo em `ExecuteInTransactionAsync` (outbox)
- [x] 2.2 Consumidores e `OrderPaymentHandlers` repassando a tentativa; hub mandando `attempt` no `PaymentStatusChanged`
- [x] 2.3 Endpoint `POST /orders/{orderNumber}/payment` com `VerifiedCustomer`: `202`, `400`, `404`, `409`; acrescentar ao `Orders.Web.Api.http`
- [x] 2.4 Testes do handler, dos consumidores (tentativa atual e atrasada) e do mapeamento do endpoint; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 3. Payments

- [x] 3.1 Repassar a tentativa do `PaymentRequested` ao comando e aos eventos de resultado (a deduplicação por pedido + tentativa vem de `add-payment-idempotency`)
- [x] 3.2 Teste: tentativa 2 do mesmo pedido gera novo pagamento; repetição da tentativa 2 não; verificar com `dotnet test src/backend/Payments/Payments.Tests`

## 4. Front-end

- [x] 4.1 Tirar o `clearCart()` do `201` e chamá-lo na `OrderConfirmation` em `approved` e `pending`
- [x] 4.2 Na recusa, `PaymentForm` com "Tentar com outro cartão" enquanto houver tentativas; cliente `retryOrderPayment` no `lib/orders.ts`; ignorar mensagens do hub de tentativa anterior
- [x] 4.3 `npm run build && npm run lint` em `src/frontend`

## 5. Verificação e documentação

- [x] 5.1 Compose com volumes recriados: cartão final `0000`, depois `1111` no mesmo pedido termina `Confirmed` com 2 pagamentos no Payments e 2 e-mails no Mailpit; 3 recusas esgotam a opção; o carrinho some só na aprovação
  - Verificado no compose, com os volumes recriados e o SMTP do orders-api apontado para o Mailpit durante o teste (depois restaurado para o Gmail): pedido gravado no orders-db como o POST /createOrder faria, cartões tokenizados no POST /card-tokens e solicitações publicadas no RabbitMQ. Final `0000` na tentativa 1 → `Failed`; nova tentativa aplicada com o mesmo efeito do `RetryPayment`; um `PaymentRejected` atrasado da tentativa 1 foi ignorado (pedido seguiu `Pending` na tentativa 2); final `1111` na tentativa 2 → `Confirmed`, com 2 pagamentos no Payments (`-1` Rejected, `-2` Approved) e 2 e-mails no Mailpit. Outro pedido com 3 recusas terminou `Failed` na tentativa 3, com 3 pagamentos e 3 e-mails (o último diz que as tentativas acabaram). Nenhuma mensagem nas filas de erro; dados de teste removidos.
  - Não verificado de ponta a ponta: o `POST /orders/{n}/payment` com token do Keycloak (coberto por `RetryOrderPaymentHandlerTests` e `RetryOrderPaymentEndpointTests`) e, no navegador logado, o formulário "Tentar com outro cartão" e o carrinho esvaziado só na aprovação ou no pendente (só build e lint).
- [x] 5.2 Atualizar `usecases/UC03-finalizar-pedido.md`, `usecases/UC04-pagar-pedido.md` e o `CLAUDE.md` (endpoint, tentativas, carrinho)
- [x] 5.3 Rodar `dotnet test Scorponok.slnx`, todos sem falhas

## Workflow follow-up

- Implementar e arquivar `add-payment-idempotency` e `tokenize-card-data` antes desta.
- Esta é a última da série. Antes de arquivar, reescrever sobre o texto já arquivado os MODIFIED que outras changes também alteram; senão, o arquivamento desfaz aquelas mudanças:
  - "Solicitação repetida" (`payment-processing`), alterado por `add-payment-idempotency`;
  - "Resultado do pagamento no pedido" (`order-placement`), alterado por `handle-stuck-payments`;
  - "Um e-mail por resultado" (`order-notification`), alterado por `limit-email-retries`.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
