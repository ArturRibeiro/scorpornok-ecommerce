# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- `Order(Guid customerId)` faz `OrderNumber = $"A{GetHashCode()}"`. Com o pedido ainda sem `Id`, o `Entity<T>.GetHashCode` cai no `object.GetHashCode`, um inteiro sem garantia de unicidade entre objetos, reinícios ou instâncias.
- `OrderConfigurations` mapeia `OrderNumber` para a coluna `Number` só como obrigatória: sem índice, sem tamanho máximo.
- O número é usado como identificador externo: resposta do `POST /createOrder`, `PaymentRequested`, chave de idempotência do Payments (`{OrderNumber}-{Attempt}`), hub `WatchOrder` e e-mail.
- O `OrderHandler` grava o pedido e a mensagem do outbox na mesma transação (`ExecuteInTransactionAsync`); uma exceção na gravação desfaz as duas.
- Sem migrations: `EnsureCreatedAsync` no `orders-db`.

## Goals / Non-Goals

**Goals:**
- Número gerado sem depender do banco, ainda no construtor do `Order` (a validação já exige o número antes de gravar).
- Repetição improvável na prática, inclusive entre reinícios e recriações do banco.
- O banco como garantia final: uma repetição falha alto em vez de misturar dois pedidos.

**Non-Goals:**
- Regerar o número e tentar de novo automaticamente numa colisão.
- Mudar o formato visível além do tamanho (continua `A` + dígitos).

## Decisions

### 1. `A` + 12 dígitos aleatórios do gerador criptográfico
`OrderNumber.New()` (função estática no domínio) junta dois blocos de 6 dígitos do `RandomNumberGenerator.GetInt32` (o primeiro entre 100000 e 999999, o segundo com zeros à esquerda), já que o .NET não tem `GetInt64`: sempre 12 dígitos, uniformes entre 100000000000 e 999999999999 (ex.: `A483920175536`). São 9 × 10¹¹ valores; com 100 mil pedidos, a chance de alguma repetição é da ordem de 0,5 %, e de um pedido novo colidir com os anteriores, cerca de 10⁻⁷.

- *Alternativa:* sequência do Postgres: nunca repete dentro do banco, mas exige ir ao banco antes de montar o pedido e reinicia com o volume, voltando a repetir números que o Payments ainda guarda.
- *Alternativa:* `Guid` completo: único na prática, mas ilegível para o cliente na tela e no e-mail.
- *Alternativa:* data + sequência curta: legível, mas com muito menos entropia por dia.

### 2. Índice único em `Number`
`OrderConfigurations`: `HasMaxLength(16)` e `HasIndex(x => x.OrderNumber).IsUnique()`. Numa colisão, o `SaveEntitiesAsync` dentro da transação lança, a transação é desfeita (pedido e `PaymentRequested`), e o `POST /createOrder` responde erro; a loja já trata isso mantendo carrinho e formulário, e o cliente pode confirmar de novo, recebendo outro número.

### 3. Construtor
`Order(Guid customerId)` passa a usar `OrderNumber.New()`. Os testes que criam pedidos não mudam; os que precisam de um número fixo continuam usando os dublês que já usam.

## Risks / Trade-offs

- [Colisão vira erro para o cliente] → probabilidade desprezível; o carrinho e o formulário ficam, e a nova tentativa gera outro número.
- [Recriar só o `orders-db` mantendo o `payment-db`] → um número novo poderia coincidir com um antigo do Payments; a entropia torna isso improvável, e o procedimento recomendado continua sendo recriar os dois volumes juntos.
- [Mudança de esquema sem migrations] → recriar o volume `orders_data` (apaga os pedidos de teste).

## Migration Plan

1. Recriar o volume do `orders-db` (`docker compose stop orders-api orders-db && docker compose rm -f orders-db && docker volume rm scorpornok-ecommerce_orders_data`) e, junto, o do `payment-db`, para não restarem pagamentos de pedidos apagados.
2. Subir de novo `payment-api` e `orders-api`.
3. Rollback: reverter o commit e recriar os volumes.
