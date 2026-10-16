# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- O `OrderEmailDispatcher` (`BackgroundService` do `orders-api`) roda a cada `Smtp:PollSeconds` (5 s), busca até 20 pedidos com `IsAwaitingPaymentEmail` (`Confirmed`/`Failed` e `Payment.EmailSentAt` nulo, critério repetido em SQL no `OrderRepository.GetAwaitingPaymentEmailAsync`), envia e marca com `MarkPaymentEmailSent()`.
- Qualquer exceção no envio faz `break` na rodada, com `LogWarning`. Não há contador nem distinção de erro: um pedido com endereço recusado fica primeiro da fila (ordem por `Id`) e bloqueia todos os outros para sempre.
- O `SmtpOrderEmailSender` abre uma conexão MailKit por e-mail: `MailboxAddress.Parse` (lança `ParseException` com endereço inválido), `ConnectAsync`, `AuthenticateAsync` (se houver usuário), `SendAsync`.
- `OrderPayment` (tabela `OrderPayment`, 1:1 com `Order`) guarda `PaymentId` e `EmailSentAt`; só o `Order` o altera.
- Health checks: `/health/ready` (tag `ready`, banco) e `/health/bus` (tag `bus`).

## Goals / Non-Goals

**Goals:**
- A classificação do erro fica na infraestrutura; o domínio só registra tentativas e o resultado.
- Nenhuma mudança no fluxo do pagamento nem no consumidor.

**Non-Goals:**
- Fila própria ou mensageria para o e-mail; o dispatcher por polling continua.
- Alterar o conteúdo do e-mail.

## Decisions

### 1. Estado do envio no `OrderPayment`
Novos campos: `EmailAttempts` (int), `NextEmailAttemptAt` (DateTime?) e `EmailFailedAt` (DateTime?), com `EmailFailureReason` (string curta). Métodos no `Order`, como o `MarkPaymentEmailSent`:
- `RegisterPaymentEmailRetry(now, delay, reason)`: incrementa as tentativas e agenda a próxima.
- `MarkPaymentEmailFailed(now, reason)`: falha definitiva.

`IsAwaitingPaymentEmail` passa a exigir `EmailFailedAt` nulo e `NextEmailAttemptAt` nulo ou vencido; o `GetAwaitingPaymentEmailAsync` repete o critério e ordena por `NextEmailAttemptAt` (nulos primeiro) e `Id`.

- *Alternativa:* tabela própria de tentativas: histórico completo, mas mais uma entidade para um contador.

### 2. Classificação do erro
O `IOrderEmailSender` passa a lançar `EmailDeliveryException` com um `Kind`, mapeado no `SmtpOrderEmailSender`:

| Erro | `Kind` | Efeito |
|---|---|---|
| `ParseException` do endereço; `SmtpCommandException` com `ErrorCode = RecipientNotAccepted` e status 5xx | `Recipient` | falha definitiva do pedido |
| `SmtpCommandException` 4xx ou `MessageNotAccepted` | `Message` | conta tentativa, intervalo crescente |
| `SocketException`, `SslHandshakeException`, `AuthenticationException`, `SmtpProtocolException`, tempo esgotado na conexão | `Server` | não conta tentativa; encerra a rodada |

Autenticação errada afeta todos os pedidos igualmente, por isso é `Server`: queimar tentativas de cada pedido levaria todos à falha definitiva por um erro de configuração.

### 3. Intervalos
- Por pedido (`Message`): `RetryBaseSeconds × 2^(tentativas−1)`, limitado a `RetryMaxSeconds`; falha definitiva ao atingir `MaxEmailAttempts`. Padrões: base 60 s, teto 1 h, 6 tentativas (≈ 30 min entre a 1ª e a última: 1, 2, 4, 8 e 16 min).
- Do servidor (`Server`): o dispatcher dobra a espera entre rodadas a partir de `PollSeconds`, até `RetryMaxSeconds`, e volta ao normal no primeiro envio bem-sucedido. Sem gravar nada nos pedidos.
- Novas chaves em `Smtp`: `MaxEmailAttempts`, `RetryBaseSeconds`, `RetryMaxSeconds`.

### 4. Rodada sem bloqueio
`Recipient` e `Message` registram o resultado no pedido e seguem para o próximo; só `Server` encerra a rodada. Cada pedido é salvo logo após o seu resultado.

### 5. Verificação de saúde
`EmailDeliveryHealthCheck` (tag `email`, mapeado em `/health/email`, fora do `ready`):
- `Unhealthy` (503) se a última rodada terminou com `Server` (estado em memória, singleton `EmailDeliveryState` atualizado pelo dispatcher).
- `Degraded` (200, com a contagem em `data`) se houver pedidos com `EmailFailedAt` nas últimas 24 h.
- `Healthy` nos demais casos.

Falha definitiva gera `LogError` com número do pedido e motivo; `Server` gera `LogWarning` só na primeira rodada falha e na recuperação, para não inundar o log.

## Risks / Trade-offs

- [Um 5xx de destinatário pode ser política temporária do provedor] → aceito: o motivo fica gravado e o caso aparece no health check.
- [Estado `Server` em memória se perde no reinício] → o primeiro envio após subir recalcula.
- [Mais colunas no `OrderPayment`] → campos simples, sem índice novo além do critério da busca.

## Migration Plan

- Recriar o volume do `orders-db` (`docker compose stop orders-api orders-db && docker compose rm -f orders-db && docker volume rm scorpornok-ecommerce_orders_data`).
- Rollback: reverter o commit e recriar o volume de novo.
