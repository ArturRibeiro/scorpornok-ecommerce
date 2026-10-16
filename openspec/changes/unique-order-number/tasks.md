# Tasks

## 1. Geração do número

- [x] 1.1 Criar `OrderNumber.New()` no `Orders.Domain` (`A` + 12 dígitos do `RandomNumberGenerator`, em dois blocos de 6 com `GetInt32`) e usá-lo no construtor `Order(Guid customerId)` no lugar do `GetHashCode()`
- [x] 1.2 Testes no `Orders.Tests`: o número tem o formato `^A\d{12}$`, e mil pedidos criados em sequência têm números distintos; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 2. Banco

- [x] 2.1 No `OrderConfigurations`, `HasMaxLength(16)` e índice único em `OrderNumber` (coluna `Number`)
- [x] 2.2 Recriar os volumes `orders_data` e `payment_data`, subir o compose e conferir no `psql` o índice único em `"order"."Orders"("Number")`; tentar inserir um número repetido e ver a violação

## 3. Verificação e documentação

- [x] 3.1 Pedido feito no navegador recebe número no formato `A` + 12 dígitos, e o pagamento usa a chave `{número}-1`
- [x] 3.2 Atualizar o `CLAUDE.md` (geração do número e índice único) e rodar `dotnet test Scorponok.slnx`

## Workflow follow-up

- Implementar e arquivar antes de `add-payment-idempotency`, cuja chave depende do número único.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
