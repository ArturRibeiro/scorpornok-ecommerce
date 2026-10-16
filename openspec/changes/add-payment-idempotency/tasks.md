# Tasks

## 1. Domínio do pagamento

- [x] 1.1 Adicionar ao `Payment` `Attempt`, `IdempotencyKey` (`"{OrderNumber}-{Attempt}"`), `Status` (`Processing`, `Approved`, `Rejected`), `RequestedAt`; `Approved` derivado do `Status`; fábrica `Start(...)` e método `Complete(PaymentResult)` que só age sobre `Processing`
- [x] 1.2 Atualizar `PaymentTests`: `Start` cria em `Processing` sem resultado, `Complete` aprova/recusa e preenche `ProcessedAt`, `Complete` repetido não altera, chave com número do pedido e tentativa, regras de dados sensíveis mantidas; verificar com `dotnet test src/backend/Payments/Payments.Tests`

## 2. Contrato e gateway simulado

- [x] 2.1 Adicionar `IdempotencyKey` ao `PaymentRequest` e documentar na `IPaymentGateway` a semântica "mesma chave → mesma resposta, sem nova cobrança"
- [x] 2.2 Fazer o `PaymentGateway` guardar o resultado por chave (`ConcurrentDictionary`) e registrá-lo como `Singleton` no `AddInfrastructure`
- [x] 2.3 Testes do gateway: a mesma chave devolve o mesmo resultado sem reavaliar, chaves diferentes são independentes, regra do `0000` mantida

## 3. Repositório e banco

- [x] 3.1 Trocar `GetByOrderNumberAsync` por `GetByIdempotencyKeyAsync` no `IPaymentRepository`/`PaymentRepository`
- [x] 3.2 No `PaymentConfiguration`, mover o índice único de `OrderNumber` para `IdempotencyKey` e mapear as colunas novas; verificar recriando o volume `payment_data` e conferindo o esquema no `psql`

## 4. Handler

- [x] 4.1 Reescrever o `ProcessPaymentHandler`: busca pela chave, grava `Processing` antes de cobrar, cobra com a chave quando `Processing`, `Complete` + grava, publica o resultado; deixar a violação do índice único subir para a retentativa
- [x] 4.2 Atualizar `ProcessPaymentHandlerTests`: ordem gravar → cobrar → completar; falha ao gravar o resultado e reentrega não cobram duas vezes (gateway chamado com a mesma chave, stub real); pagamento já completo só republica; aprovação e recusa publicam o evento certo; verificar com `dotnet test src/backend/Payments/Payments.Tests`

## 5. Verificação ponta a ponta e documentação

- [x] 5.1 Com o compose no ar: pedido com cartão final `1111` termina `Confirmed` e com `0000` termina `Failed`; reenviar o mesmo `PaymentRequested` pelo painel do RabbitMQ não cria outro pagamento
- [x] 5.2 Atualizar o `CLAUDE.md` (Payments: chave de idempotência, situação "em processamento", índice único na chave, gateway simulado guarda resultado por chave) e rodar `dotnet test Scorponok.slnx`

## Workflow follow-up

- Arquivar só depois de `unique-order-number`: a chave `{OrderNumber}-{Attempt}` depende de o número do pedido ser único.
- Implementar antes de `allow-payment-retry`, que depende desta change.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
