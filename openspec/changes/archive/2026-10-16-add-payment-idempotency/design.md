# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- `ProcessPaymentHandler`: `GetByOrderNumberAsync` → se nulo, `IPaymentGateway.SendProcessPaymentAsync` → `Payment.Create(..., result)` → `Add` → `SaveEntitiesAsync` → publica `PaymentApproved`/`PaymentRejected`. A cobrança acontece antes de qualquer gravação.
- `Payment` não tem situação intermediária: `Approved` (bool) e `Reason` são definidos na criação. `PaymentConfiguration` tem índice único em `OrderNumber` ("um pagamento por pedido").
- `IPaymentGateway.SendProcessPaymentAsync(PaymentRequest)`: o `PaymentRequest` não tem chave. O `PaymentGateway` (stub) é registrado como `Scoped` e não guarda estado.
- O consumidor tem `UseMessageRetry(Interval(3, 1 s))`; a entrega é "ao menos uma vez" e o Orders já ignora resultado repetido (pedido fora de `Pending`).
- Sem migrations: `EnsureCreatedAsync` no `payment-db`.

## Goals / Non-Goals

**Goals:**
- Nenhuma sequência de falha ou concorrência do Payments gera duas cobranças para a mesma tentativa.
- O contrato com o Orders (`PaymentRequested`, `PaymentApproved`, `PaymentRejected`) não muda.
- A chave já comporta várias tentativas por pedido, para a `allow-payment-retry`.

**Non-Goals:**
- Resolver pagamento preso "em processamento" quando a mensagem vai para a fila de erro (fica para `handle-stuck-payments`).
- Persistir as chaves do gateway simulado.

## Decisions

### 1. Chave de idempotência = `"{OrderNumber}-{Attempt}"`
Novo valor no domínio (`Payment.IdempotencyKey`, `Attempt`). Nesta change `Attempt` é sempre `1`; a `allow-payment-retry` passa a recebê-lo na mensagem. O índice único sai de `OrderNumber` e vai para `IdempotencyKey`, o que libera vários pagamentos por pedido sem perder a garantia por tentativa.

- *Alternativa:* chave = só `OrderNumber`: mais simples agora, mas obrigaria a mudar índice e chave de novo na próxima change.
- *Alternativa:* `Guid` gerado pelo Orders e enviado na mensagem: muda o contrato entre contextos sem necessidade, já que número do pedido + tentativa é determinístico.

### 2. Registro "em processamento" antes de cobrar
`Payment` ganha `Status` (`Processing`, `Approved`, `Rejected`); `Approved` vira propriedade derivada, e `ProcessedAt` passa a ser preenchido junto com o resultado (nova `RequestedAt` na criação). Fábrica `Payment.Start(...)` cria em `Processing`; `Complete(PaymentResult)` só age sobre `Processing`.

Fluxo do handler:
1. Busca pelo `IdempotencyKey`. Se não existe, `Start` → `Add` → `SaveEntitiesAsync` (grava antes de cobrar).
2. Se o `SaveEntitiesAsync` falhar por violação do índice único (outra entrega gravou primeiro), relança: a retentativa do MassTransit encontra o registro e segue pelo passo 3.
3. Se o pagamento está `Processing`, chama o gateway com a chave (`PaymentRequest.IdempotencyKey`). O gateway devolve o resultado original se a chave já foi usada.
4. `Complete(result)` → `SaveEntitiesAsync`.
5. Publica o resultado (como hoje, também quando o pagamento já estava completo).

- *Alternativa:* lock pessimista (`SELECT ... FOR UPDATE`) em volta da cobrança: segura uma transação aberta durante a chamada externa e não cobre o processo derrubado entre cobrar e gravar.
- *Alternativa:* outbox no Payments: resolve a publicação, não a cobrança duplicada.

### 3. Contrato do gateway com chave
`PaymentRequest` ganha `IdempotencyKey`. A semântica exigida de `IPaymentGateway` (comentário na interface): mesma chave → mesma resposta, sem nova cobrança; é o modelo dos gateways reais (cabeçalho de idempotência).

O `PaymentGateway` simulado guarda o `PaymentResult` por chave num `ConcurrentDictionary` e passa a ser registrado como `Singleton` (no `Scoped` o dicionário se perderia a cada mensagem). A regra do final `0000` continua.

## Risks / Trade-offs

- [Gateway simulado perde as chaves ao reiniciar] → reentrega depois de um reinício cobra "de novo" no stub; sem efeito real. Registrado como limitação; um gateway real guarda a chave do lado dele.
- [Pagamento fica `Processing` se a mensagem esgotar as retentativas] → sem cobrança duplicada, mas sem resultado; tratado pela `handle-stuck-payments`.
- [Duas entregas simultâneas chamam o gateway] → a segunda só chama depois que a primeira gravou o `Processing`, com a mesma chave; o gateway devolve o mesmo resultado.
- [Mudança de esquema sem migrations] → recriar o volume `payment_data` (apaga pagamentos de teste).

## Migration Plan

1. Recriar o volume do `payment-db`: `docker compose stop payment-api payment-db && docker compose rm -f payment-db && docker volume rm scorpornok-ecommerce_payment_data`.
2. Subir o `payment-api`. Pedidos `Pending` antigos cujo pagamento já existia podem ser reprocessados se a mensagem for reentregue; em desenvolvimento, recriar também o `orders-db` evita a inconsistência.
3. Rollback: reverter o commit e recriar o volume de novo.
