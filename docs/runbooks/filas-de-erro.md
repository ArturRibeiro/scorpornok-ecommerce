# Runbook: mensagens nas filas de erro

Quando um consumidor esgota as retentativas (cerca de 5 minutos, veja o `TransientRetry`), o MassTransit move a mensagem para a fila `<fila>_error`, que ninguém lê. O `/health/ops` do Orders e do Payments acusa isso com `503` e cita a fila e a quantidade.

| API | `/health/ops` | Filas de erro verificadas |
|---|---|---|
| Orders | http://localhost:5224/health/ops | `payment-approved_error`, `payment-rejected_error`, `payment-not-processed_error`, `payment-status-pending_error` (e pedidos presos) |
| Payments | http://localhost:5220/health/ops | `payment-requested_error`, `payment-status-requested_error` |

Mesmo sem ninguém mexer na fila de erro, o pedido não fica preso para sempre: depois do prazo (`PaymentCheck:TimeoutMinutes`), o Orders consulta o Payments, que republica o resultado registrado ou anula o pedido sem cobrar. O runbook serve para limpar a fila de erro e entender a causa.

## 1. Entender a causa

1. Abra o painel do RabbitMQ (http://localhost:15672, `guest`/`guest`) → **Queues and Streams** → a fila `_error`.
2. Em **Get messages**, use *Ack mode* = `Nack message requeue true` (só lê, não tira da fila) e veja os cabeçalhos:
   - `MT-Fault-Message`: a mensagem da exceção (ex.: banco fora do ar, tempo esgotado);
   - `MT-Fault-ExceptionType` e `MT-Fault-StackTrace`: o tipo e onde aconteceu;
   - `MT-Reason`: `fault` quando esgotou as tentativas.
3. Corrija a causa antes de reprocessar (suba o banco, o serviço ou corrija a configuração). Uma `ArgumentException` (mensagem malformada) vai direto para a fila de erro e não adianta reprocessar: avalie a mensagem e descarte.

## 2. Devolver as mensagens à fila de origem

É seguro reprocessar:
- o Payments não cobra de novo uma tentativa já registrada (chave de idempotência `{pedido}-{tentativa}`);
- o Orders ignora resultado para pedido que já tem desfecho (confirmado, recusado ou não processado).

**Opção A: script, sem plugin** (a imagem do compose não traz o shovel habilitado):

```bash
python3 docs/runbooks/mover-fila-de-erro.py payment-approved_error --dry-run   # só mostra quantas
python3 docs/runbooks/mover-fila-de-erro.py payment-approved_error             # move todas
```

**Opção B: painel do RabbitMQ**, com o plugin de shovel:

```bash
docker exec rabbitmq rabbitmq-plugins enable rabbitmq_shovel rabbitmq_shovel_management
```

Depois, na fila `_error`, use **Move messages** com o destino sem o sufixo `_error` (ex.: `payment-approved`).

## 3. Conferir

As contagens da API de gerenciamento (e, por isso, o `/health/ops`) levam alguns segundos para refletir uma fila que acabou de mudar.

- O `/health/ops` volta a `200`.
- O pedido tem o desfecho esperado no `orders-db` (`"order"."Orders"`, coluna `StatusName`).
- Se a fila de erro voltar a encher, a causa não foi resolvida: repita o passo 1.
