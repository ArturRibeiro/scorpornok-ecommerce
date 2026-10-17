# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- `CheckoutPage` chama `clearCart()` no `201` do `POST /createOrder`; a `OrderConfirmation` acompanha o hub e termina em `approved`, `declined` ou `pending` (10 s sem mensagem).
- `Order.ConfirmPayment`/`FailPayment` só agem sobre `Pending`; `Failed` é definitivo. O `OrderPayment` (1:1, chave `OrderId`) guarda `PaymentId` e `EmailSentAt`, e o `OrderEmailDispatcher` envia para `Confirmed`/`Failed` com `EmailSentAt` nulo.
- `PaymentRequested(OrderId, OrderNumber, CustomerId, Amount, Installments, Card)`, `PaymentApproved(OrderNumber, PaymentId)` e `PaymentRejected(OrderNumber, Reason)` não identificam a tentativa.
- O Payments deduplica por `OrderNumber` (índice único no `payment-db` e `GetByOrderNumberAsync` no handler).
- `add-payment-idempotency` troca a chave de deduplicação do Payments; `tokenize-card-data` troca os dados do cartão por um token.

## Goals / Non-Goals

**Goals:**
- O pedido continua sendo o agregado que decide as transições; o endpoint só traduz HTTP.
- Um resultado atrasado de outra tentativa nunca altera o pedido.
- A nova solicitação usa o mesmo outbox do registro do pedido.

**Non-Goals:**
- Guardar o histórico de tentativas no Orders (o Payments já guarda um pagamento por tentativa).
- Mudar o total ou os itens na nova tentativa.

## Decisions

### 1. Tentativa atual no `OrderPayment`
`OrderPayment` ganha `Attempt` (começa em 1). `Order.RetryPayment()`: só a partir de `Failed` e com `Attempt < MaxAttempts` (3); volta a `Pending`, incrementa `Attempt` e zera `EmailSentAt`, para que o resultado da nova tentativa gere o seu e-mail. `ConfirmPayment(paymentId, attempt)` e `FailPayment(attempt)` passam a exigir `attempt == Payment.Attempt`, além de `Pending`.

- *Alternativa:* uma linha por tentativa no Orders (1:N): permite histórico, mas o Payments já o tem e o `OrderPayment` 1:1 é recente.
- *Alternativa:* criar um pedido novo a cada tentativa: simples no domínio, mas o cliente veria números diferentes e e-mails de pedidos "falhos" que não existem para ele.

### 2. Número da tentativa nos contratos
`PaymentRequested`, `PaymentApproved` e `PaymentRejected` ganham `int Attempt` (padrão 1, para mensagens antigas na fila). O Payments repassa a tentativa recebida no resultado.

### 3. Endpoint `POST /orders/{orderNumber}/payment`
Corpo: o cartão (token de `tokenize-card-data`) e as parcelas. Política `VerifiedCustomer`. Respostas: `202` com `{ orderNumber, status, attempt }`; `404` para pedido inexistente ou de outro cliente (não revela a existência, como o hub); `409` se o pedido não está `Failed` ou se as tentativas acabaram; `400` para cartão inválido. Novo command `RetryOrderPaymentCommand` com handler que, dentro de `ExecuteInTransactionAsync`, chama `RetryPayment()`, publica o `PaymentRequested` com o mesmo `Total` e grava (outbox).

### 4. Carrinho e tela
- `clearCart()` sai do `201` e passa a ser chamado quando a `OrderConfirmation` chega a `approved` ou `pending`. No `pending`, o pedido já existe e pode ser aprovado depois; manter o carrinho levaria a um segundo pedido igual.
- Em `declined`, a `OrderConfirmation` mostra o `PaymentForm` com "Tentar com outro cartão" enquanto houver tentativas. O resultado da nova tentativa é acompanhado pelo mesmo hub (`WatchOrder`), que passa a mandar `attempt` no `PaymentStatusChanged`; a tela ignora mensagens de tentativa anterior.
- `CreatedOrder` ganha `attempt` e `maxAttempts`, para a tela saber se oferece a opção.

### 5. E-mail
Sem mudança no dispatcher: como `RetryPayment()` zera `EmailSentAt`, o resultado de cada tentativa entra no critério `IsAwaitingPaymentEmail`. O texto do e-mail de recusa menciona a nova tentativa quando ainda houver.

## Risks / Trade-offs

- [Mensagem antiga na fila sem `Attempt`] → padrão 1; o Orders a trata como tentativa 1 e a ignora se o pedido já estiver em outra.
- [Cliente fecha a página após a recusa] → sem histórico de pedidos na loja, ele não volta à tela de nova tentativa; o carrinho continua no navegador para um novo pedido. Registrado como fora do escopo.
- [Mais e-mails] → até 3 por pedido, um por resultado.
- [Volumes recriados] → o modelo do EF muda no Orders e no Payments; os pedidos e pagamentos de teste se perdem.

## Migration Plan

- Arquivar `add-payment-idempotency` e `tokenize-card-data` antes desta.
- Recriar os volumes `orders_data` e `payment_data`.
- Rollback: reverter o commit e recriar os volumes de novo.
