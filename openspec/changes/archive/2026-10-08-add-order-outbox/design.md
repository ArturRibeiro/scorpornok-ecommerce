# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- `OrderHandler` faz `orderRepository.Save(order)` → `UnitOfWork.SaveEntitiesAsync` → `publisher.PublishAsync(PaymentRequested)`, com `try/catch` em volta da publicação. O `MassTransitIntegrationEventPublisher` chama `IPublishEndpoint.Publish`, que, com o broker fora, não falha: fica esperando a conexão, e o `catch` nunca é acionado.
- `PaymentRequested` leva `OrderId` (o `int` gerado pelo banco), gravado pelo Payments em `Payment.OrderId`. O `Id` do pedido só existe depois do primeiro `SaveChanges`.
- `IUnitOfWork` (Shared) só tem `SaveChangesAsync`/`SaveEntitiesAsync`, sem transação explícita. `OrderContext` usa o schema `order` e `EnsureCreatedAsync` (sem migrations).
- Os consumidores do Orders (`PaymentApproved`/`PaymentRejectedConsumer`) não publicam nada; a única publicação do Orders é a do `OrderHandler`.

## Goals / Non-Goals

**Goals:**
- Os handlers continuam sem depender do MassTransit: o `OrderHandler` segue usando `IIntegrationEventPublisher`.
- Uma única transação no `orders-db` cobre o pedido e a mensagem.

**Non-Goals:**
- Mudar o contrato `PaymentRequested` ou qualquer código do Payments.
- Usar o outbox de consumidor (`UseEntityFrameworkOutbox` nos endpoints) do Orders.

## Decisions

### 1. Bus outbox do MassTransit com EF Core
Pacote `MassTransit.EntityFrameworkCore` (`$(MassTransit)`) no `Orders.Infrastructure`. No `AddMassTransit`:

```csharp
x.AddEntityFrameworkOutbox<OrderContext>(o =>
{
    o.UsePostgres();
    o.UseBusOutbox();
});
```

No `OrderContext.OnModelCreating`: `AddInboxStateEntity()`, `AddOutboxMessageEntity()` e `AddOutboxStateEntity()`, no schema `order`.

Com o `UseBusOutbox`, o `IPublishEndpoint` resolvido no escopo da requisição deixa de enviar ao broker: ele adiciona a mensagem ao `OrderContext`, e ela é gravada no próximo `SaveChanges`. O `BusOutboxDeliveryService<OrderContext>` (um `IHostedService` registrado pelo MassTransit, dentro do `orders-api`) lê as mensagens confirmadas, publica no RabbitMQ e só as remove depois da confirmação do broker. O lock do Postgres impede entrega duplicada entre instâncias.

O `MassTransitIntegrationEventPublisher` não muda: ele já recebe o `IPublishEndpoint` por injeção, no mesmo escopo do `OrderContext`.

- *Alternativa:* outbox próprio (tabela + `BackgroundService` nosso) — mais didático, mas reimplementa lock, retry e limpeza que o MassTransit já tem testados.
- *Alternativa:* tempo-limite no `PublishAsync` + rotina que republica pedidos `Pending` antigos — o `POST` volta a responder rápido, mas a mensagem se perde e a recuperação depende de mais código.

### 2. Transação explícita para manter o `OrderId`
Como o `Id` do pedido só existe depois do `SaveChanges`, o handler grava em dois passos dentro de uma transação:

1. `Save(order)` → `SaveEntitiesAsync` (gera o `Id`);
2. `PublishAsync(PaymentRequested)` → `SaveEntitiesAsync` (grava a mensagem no outbox);
3. commit.

Para isso, `IOrderRepository` ganha `Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken)`, implementado no `OrderRepository` com `OrderContext.Database.BeginTransactionAsync` (commit no fim, rollback em exceção). A abstração fica no Orders; o `IUnitOfWork` do Shared não muda.

O `try/catch` em volta da publicação sai: com o outbox, `PublishAsync` não depende do broker, e qualquer exceção ali é falha real, que deve desfazer o pedido e virar erro na resposta (requisito "Pedido e solicitação de pagamento atômicos"). Pedido sem cartão continua salvo, sem publicação e com log de aviso.

- *Alternativa:* chave do pedido por HiLo/sequência (`Id` atribuído no `Add`) — um único `SaveChanges`, mas muda a geração de chaves do `Order` para contornar uma questão de ordem.
- *Alternativa:* tirar `OrderId` do `PaymentRequested` — muda o contrato e o Payments, fora do escopo.

### 3. Testes
- Handler (unitário, com mocks): `ExecuteInTransactionAsync` do mock executa o delegate; os testes verificam a ordem `Save` → `SaveEntitiesAsync` → `PublishAsync` → `SaveEntitiesAsync`, com tudo dentro da transação; publicador que lança exceção faz o handler propagar a exceção (substitui o teste "exceção do publicador ainda devolve sucesso"); pedido sem cartão não publica.
- Outbox (ponta a ponta, no compose): broker parado, reinício da API e fluxo normal, como nas tarefas.

## Risks / Trade-offs

- [Publicar depois do último `SaveChanges` perde a mensagem sem erro] → a ordem é coberta por teste do handler e por um comentário no código.
- [Entrega "ao menos uma vez": a mesma mensagem pode sair duas vezes] → o Payments já trata solicitação repetida (só republica o resultado) e o Orders ignora resultado para pedido não `Pending`.
- [Mensagem entregue antes de a fila `payment-requested` existir é descartada pelo RabbitMQ] → igual a hoje; continua documentado que o `payment-api` precisa subir ao menos uma vez.
- [Atraso entre o commit e a entrega (intervalo de leitura do serviço)] → aceitável: o checkout já espera o resultado pelo hub por até 30 s.
- [O cartão, inclusive o CVV, passa a ficar gravado no `orders-db` até a entrega] → hoje ele já trafega na fila; a mensagem é removida após a entrega. Tokenização continua fora do escopo e o risco fica registrado no `CLAUDE.md`.

## Migration Plan

1. Subir o código novo e recriar o volume do `orders-db` (`docker compose stop orders-api orders-db && docker compose rm -f orders-db && docker volume rm scorpornok-ecommerce_orders_data`), para que o `EnsureCreatedAsync` crie as tabelas do outbox.
2. Rollback: voltar o código; as tabelas extras ficam sem uso e não atrapalham.
