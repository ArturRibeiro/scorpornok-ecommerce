# Design

## Context

A motivação está em `proposal.md`; os requisitos, nos deltas `payment-processing`, `order-placement` e `checkout`. O estado atual que molda a abordagem:

- **Orders**: o `OrderHandler` valida, salva e responde `CreateOrderResult`; o `card` do `CreateCommand` é ignorado. `Order` tem `PaymentId` (privado, sempre nulo), `Status` como owned type com instâncias estáticas (`OrderStatus.Pending/Confirmed/Failed`) e `ChangeStatus`. `IOrderRepository` só tem `Save`. `OrderNumber` é `"A" + GetHashCode()`, sem índice único.
- **Payments**: `Gateway.Payment.Domain` tem `PaymentMethod` (guarda CVV e número completo), `IPaymentGateway`, `PaymentRequest`/`PaymentResult`; `Gateway.Payment.Data` tem o event store (sem provider configurado), o stub `PaymentGateway` e `CreditCardPaymentConfiguration`, sem DbContext que os use. `Gateway.Payment.Web.Api` é o template de previsão do tempo; o compose já passa a connection string do `payment-db`, que a API não lê.
- **Shared**: `IMemoryBus` com `SendAsync`/`RequestAsync`; `RaiseEvent` está comentado. Não há broker nem biblioteca de mensageria.
- **Front-end**: `OrderConfirmation` mostra o resultado do `201` (sempre `Pending`). O front fala direto com a API de Orders (porta 5224), liberada no CORS sem credenciais.

## Goals / Non-Goals

**Goals:**
- Mensageria só nas bordas (Infrastructure e Web.Api); domínio e handlers continuam testáveis sem broker.
- Manter o padrão do projeto: entrada (endpoint ou consumidor) → `IMemoryBus` → handler MediatR.

**Non-Goals:**
- Reaproveitar o event store (`EventStoreContext`/`SqlEventStore`) para o pagamento.
- Religar `IMemoryBus.RaiseEvent` (eventos de domínio em memória) — integração entre contextos é outra coisa.

## Decisions

### 1. MassTransit 8.5.10 sobre RabbitMQ
`MassTransit` e `MassTransit.RabbitMQ` 8.5.10 (Apache-2.0, alvo net10.0) como `$(MassTransit)` no `Directory.Build.props`. Configuração em cada API: `AddMassTransit` com os consumidores, `SetKebabCaseEndpointNameFormatter`, `UsingRabbitMq` lendo a seção `RabbitMq` (`Host`, `Username`, `Password`; padrão `localhost`/`guest`/`guest` no `appsettings.Development.json`), `UseMessageRetry` (3 tentativas, 1 s) e `ConfigureEndpoints`. Filas duráveis, criadas pelos consumidores.
- *Alternativa:* `RabbitMQ.Client` puro — mais didático, mas exige escrever topologia, serialização, retry e reconexão.
- *Alternativa:* MassTransit 9 — licença comercial.

### 2. Contratos de integração em `Shared.Code/IntegrationEvents`
O MassTransit roteia pelo nome completo do tipo, então publicador e consumidor precisam do mesmo tipo. Os contratos ficam em `Shared.Code` (já referenciado pelos dois lados), namespace `Shared.Code.IntegrationEvents`:
- `PaymentRequested(int OrderId, string OrderNumber, Guid CustomerId, decimal Amount, int Installments, CardData Card)` e `CardData(CardHolderName, CardNumber, ExpirationMonth, ExpirationYear, Cvv)`;
- `PaymentApproved(string OrderNumber, Guid PaymentId)`;
- `PaymentRejected(string OrderNumber, string Reason)`.

Para publicar sem depender de MassTransit nos handlers: `IIntegrationEventPublisher.PublishAsync<T>(T message, CancellationToken)` em `Shared.Code`, implementado em cada Infrastructure sobre `IPublishEndpoint`.
- *Alternativa:* projeto `Shared.Contracts` próprio — mais isolado, mas mais um projeto para dois arquivos.

### 3. Orders: publicar depois de salvar, consumir por comando, avisar a loja
- `OrderHandler`: depois de `SaveEntitiesAsync`, publica `PaymentRequested` com `Amount = order.Total` (o `card.amount` do cliente é ignorado) e o `card`. Se a publicação lançar exceção, registra em log e responde `201` mesmo assim (o pedido fica `Pending`; ver riscos).
- Domínio: `Order.ConfirmPayment(Guid paymentId)` e `Order.FailPayment()`, que só agem quando o status é `Pending` e devolvem se mudaram algo.
- `IOrderRepository.GetByNumberAsync(string, CancellationToken)`.
- Consumidores finos em `Orders.Infrastructure/Consumers` (`PaymentApprovedConsumer`, `PaymentRejectedConsumer`) enviam `ConfirmOrderPaymentCommand`/`FailOrderPaymentCommand` pelo `IMemoryBus`; os handlers em `Orders.CommandHandlers` carregam o pedido, chamam o domínio e salvam. Pedido inexistente: log e ignora (não adianta reentregar).
- Aviso à loja: `IOrderPaymentNotifier.NotifyAsync(orderNumber, approved)` em `Orders.CommandHandlers`, chamado pelos handlers de confirmação e falha só quando o pedido mudou. A implementação fica no `Orders.Web.Api` (decisão 6).
- `GetOrderStatusQuery : Message<string?>` devolve o status atual pelo número; é usado pelo hub para responder a quem começa a acompanhar um pedido já resolvido.

### 4. Payments: domínio novo, handler e consumidor
- `Gateway.Payment.Domain`: `PaymentMethod` vira `Payment : Entity<Guid>, IAggregateRoot` com `OrderId`, `OrderNumber`, `Amount`, `Installments`, `CardHolderName`, `CardLast4`, `Approved`, `Reason`, `ProcessedAt`. A fábrica recebe o número completo só para extrair os 4 últimos dígitos; não há campo para CVV. `PaymentResult` vira `(bool Approved, string? Reason)`. `IPaymentRepository` com `GetByOrderNumberAsync`, `Add` e `UnitOfWork`.
- Novo `Gateway.Payment.CommandHandlers`: `ProcessPaymentCommand` + handler. Se já existe pagamento para o número do pedido, republica o resultado registrado; senão chama o gateway, grava o `Payment` e publica `PaymentApproved`/`PaymentRejected`.
- `Gateway.Payment.Data`: `PaymentContext` (Npgsql, schema `payment`, `IUnitOfWork`), `PaymentConfiguration` (substitui `CreditCardPaymentConfiguration`, índice único em `OrderNumber`), `PaymentRepository`, `PaymentGateway` com a regra do `0000`, consumidor `PaymentRequestedConsumer` e `AddInfrastructure`.
- `Gateway.Payment.Web.Api`: sai o template; connection string obrigatória, `EnsureCreatedAsync` no start, health `ready` com o banco, `public partial class Program`.
- *Alternativa:* lógica direto no consumidor, sem projeto de handlers — menos projetos, mas quebra a simetria com o Orders e mistura transporte com regra.

### 5. Infra
`docker-compose.messaging.yml` (incluído pelo `docker-compose.yml`, como os de banco e front) com `rabbitmq:4.1-management`, portas 5672 e 15672, healthcheck `rabbitmq-diagnostics -q ping`. `orders-api` e `payment-api` recebem `RabbitMq__Host: rabbitmq` e dependem dele saudável.

### 6. Aviso em tempo real: hub SignalR no Orders
O navegador não fala com o RabbitMQ (expor o broker e suas credenciais ao front não é aceitável). Quem recebe o resultado do broker é o Orders, que repassa ao navegador:
- `Orders.Web.Api`: `OrderHub` em `/hubs/orders` com o método `WatchOrder(orderNumber)`, que põe a conexão no grupo `order:{orderNumber}` e, se o pedido já não está `Pending` (via `GetOrderStatusQuery`), envia o resultado na hora. `SignalROrderPaymentNotifier` implementa `IOrderPaymentNotifier` com `IHubContext<OrderHub>`, enviando `PaymentStatusChanged { orderNumber, approved }` ao grupo.
- O Orders envia depois de salvar o pedido, então a loja nunca recebe um resultado que o pedido ainda não reflete.
- CORS do Orders passa a `AllowCredentials()` (exigido pelo SignalR com `WithOrigins`).
- Front-end: `@microsoft/signalr` 10.0.11; `lib/orderHub.ts` com `watchOrderPayment(orderNumber, signal)`, que conecta, chama `WatchOrder`, resolve com `approved` na primeira `PaymentStatusChanged` daquele número e encerra a conexão. `OrderConfirmation` usa um tempo-limite de 30 s; falha de conexão ou tempo esgotado viram "ainda em processamento". O motivo da recusa não é enviado.
- *Alternativa:* hub no Payments — o front falaria com duas APIs e poderia receber o resultado antes de o pedido ser atualizado.
- *Alternativa:* Server-Sent Events — só servidor→cliente, sem a chamada `WatchOrder`; daria para usar, mas exigiria escrever grupos e reconexão à mão.
- *Alternativa:* RabbitMQ Web STOMP direto no navegador — expõe o broker.

## Risks / Trade-offs

- [Dados do cartão, inclusive CVV, trafegam e ficam na fila do RabbitMQ] → aceito para estudo; nada sensível é persistido no Payments. Criptografia de mensagem ou tokenização ficam fora do escopo.
- [Publicação falha depois de salvar → pedido fica `Pending` para sempre] → log do erro; outbox transacional (`MassTransit.EntityFrameworkCore`) fica para uma change futura.
- [Mensagem publicada antes de a fila do Payments existir é descartada pelo RabbitMQ] → as filas são duráveis e criadas no primeiro start do `payment-api`; no compose ele sobe junto. Documentar no `CLAUDE.md`.
- [Instâncias estáticas de `OrderStatus` como owned type] → cada consumidor/endpoint usa um `OrderContext` por escopo com um único pedido rastreado; testar a transição `Pending → Confirmed` contra o banco real.
- [`OrderNumber` sem garantia de unicidade] → colisão é improvável (hash de 32 bits); a busca por número (consumidores e `WatchOrder`) pega o primeiro. Índice único fica para quando o banco do Orders for recriado por outra razão.
- [A mensagem ao navegador pode ser perdida (aba fechada, conexão caída)] → o pedido no banco é a fonte da verdade; quem reconecta e chama `WatchOrder` recebe o resultado atual.
- [Uma instância só] → grupos do SignalR vivem na memória do processo; escalar o Orders exige backplane (fora do escopo).
- [Entrega pelo menos uma vez] → idempotência nos dois lados: Payments por `OrderNumber` (índice único) e Orders só altera pedidos `Pending`.

## Migration Plan

1. Subir o RabbitMQ e o `payment-api` (cria as filas e a tabela `payment.Payments`).
2. Subir o `orders-api` e depois o front-end.
Rollback: reverter as imagens; pedidos criados durante o rollback ficam `Pending`. O banco do Orders não muda de esquema.

