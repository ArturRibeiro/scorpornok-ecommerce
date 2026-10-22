# Padrões de projeto

Os padrões de arquitetura e de código usados no backend, onde cada um aparece e para que serve. Os caminhos são relativos a `src/backend/`.

## Arquitetura

| Padrão | Onde | Para quê |
| --- | --- | --- |
| Camadas por bounded context | Catalog, Store (Orders), Payments e Customers: `*.Domain` ← `*.CommandHandlers`/`*.Queries` ← `*.Infrastructure` ← `*.Web.Api` | As dependências apontam para o domínio, que não conhece banco, broker nem HTTP. |
| Database per service | `docker-compose.database.yml` (`catalog-db`, `orders-db`, `payment-db`, `customers-db`) | Cada contexto é dono dos seus dados; os outros só o acessam por HTTP ou por mensagens. |
| API Gateway (GraphQL Federation) | `Gateway/Gateway.Web.Api`, com o Hot Chocolate Fusion sobre os subgraphs `catalog`, `customers`, `orders` e `payments` | Um ponto de entrada único para a loja, que repassa o token a cada subgraph. |
| Ports and Adapters (Hexagonal) | `ICatalogProducts` → `HttpCatalogProducts`; `IPaymentGateway` → `PaymentGateway`; `IOrderEmailSender` → `SmtpOrderEmailSender`; `IIntegrationEventPublisher` → MassTransit | Os handlers dependem de interfaces; a Infrastructure as implementa e pode ser trocada nos testes. |

## DDD tático

| Padrão | Onde | Para quê |
| --- | --- | --- |
| Entity | `Shared/Shared.Code/Models/Entity.cs` | Identidade e acúmulo dos erros de validação da entidade. |
| Value Object | `Shared/Shared.Code/Models/ValueObject.cs` (`Address`, `OrderAddress`, `OrderStatus` etc.) | Igualdade por valor, sem identidade própria. |
| Aggregate Root | `IAggregateRoot` em `Order` (com `OrderItem` e `OrderPayment`), `Payment` e `Customer` | As partes do agregado só mudam pela raiz, que garante as regras (ex.: `Order.ConfirmPayment`, `Order.RetryPayment`). |
| Repository | `IOrderRepository`, `IPaymentRepository`, `ICustomerRepository` | Persistência do agregado inteiro, escondendo o EF Core do domínio. |
| Unit of Work | `Shared/Shared.Code/Models/IUnitOfWork.cs` (`SaveEntitiesAsync` sobre o `DbContext`) | Grava as mudanças do caso de uso de uma vez. |
| Domain Notification | `Shared/Shared.Code/Notifications/DomainNotification.cs` e `DomainNotificationHandler.cs` | Leva os erros de domínio ao chamador sem exceção. |
| Smart Enum (Enumeration class) | `Store/Orders.Domain/Order/Orders/OrderStatus.cs` | Situações do pedido como instâncias estáticas com código, nome e descrição, em vez de um `enum`. |
| Factory Method | `Payment.Start`, `Payment.Void`, `OrderNumber.New()`, `OrderBuyer.Create` | Criação do objeto já num estado válido, com a regra no próprio tipo. |

## CQRS e mensagens

| Padrão | Onde | Para quê |
| --- | --- | --- |
| CQRS | Catalog no lado de leitura (`Catalog.Queries/.../IProductQueries.cs`); Orders e Payments no de escrita | Leitura e escrita com modelos e caminhos separados. |
| Mediator | MediatR, envolvido por `Shared/Shared.Code/IMemoryBus.cs` e `Imp/Bus/MemoryBus.cs` (`SendAsync`, `RequestAsync`) | Endpoints e consumidores não conhecem os handlers. |
| Command / Command Handler | `Shared/Shared.Code/Commands/CommandBase.cs`, `Message.cs`; handlers como `OrderHandler` e `ProcessPaymentHandler` | Cada caso de uso é uma mensagem com um handler. |
| Publish/Subscribe (Integration Events) | `Shared/Shared.Code/IntegrationEvents/` (`OrderPlaced`, `PaymentRequested`, `PaymentApproved`, `PaymentRejected`...) via MassTransit e RabbitMQ | Os contextos se comunicam por eventos, sem chamada direta. |
| Transactional Outbox | `Store/Orders.Infrastructure/Extensions/ServiceCollectionExtensions.Messaging.cs` (`AddEntityFrameworkOutbox<OrderContext>`) | Pedido e mensagens gravados na mesma transação; o pedido não depende do broker no ar. |
| Idempotent Consumer | Payments: chave `{OrderNumber}-{Attempt}` com índice único; Orders ignora resultado de pedido já resolvido; Customers ignora pedido já registrado | Uma mensagem entregue de novo não cobra nem altera nada duas vezes. |
| Retry com backoff exponencial | `Messaging/TransientRetry.cs` de cada `*.Infrastructure`; reenvio do e-mail no `OrderEmailDispatcher` | Falhas curtas se resolvem sozinhas (3 s a 2 min, cerca de 5 min no total). |
| Dead Letter Channel | Filas `<fila>_error`, vigiadas pelo `Monitoring/ErrorQueuesHealthCheck.cs` | Mensagens que esgotaram as tentativas ficam guardadas e geram alerta. |
| Saga coreografada (com timeout) | Pedido → `PaymentRequested` → `PaymentApproved`/`PaymentRejected`; `Store/Orders.Infrastructure/PaymentCheck/PendingOrderWatcher.cs` | O fluxo segue por eventos, sem orquestrador; o pedido parado além do prazo é consultado e resolvido (`NotProcessed`). |
| Event Sourcing (sem uso) | `Payments/Payments.Infrastructure/EventSourcing/SqlEventStore.cs` e `Context/EventStoreContext.cs` | Implementação antiga, mantida no código mas não usada pelo fluxo atual. |

## Padrões GoF e de código

| Padrão | Onde | Para quê |
| --- | --- | --- |
| Builder (fluente) | `Store/Orders.CommandHandlers/Orders/Imp/CommandHandlers/OrderBuilder.cs` | Monta o `Order` passo a passo (e-mail, endereço, itens) antes da validação. |
| Template Method | `Shared/Shared.Code/Commands/CommandHandler.cs`; `BaseWebApplicationFactory` dos testes, especializada para Postgres, Oracle e Sqlite | A base define o roteiro e as subclasses preenchem os passos. |
| Chain of Responsibility | `AuthorizationForwardingHandler` (`DelegatingHandler`) em `Gateway/Gateway.Web.Api/Authentication/GatewayAuthentication.cs`; middlewares do ASP.NET Core | Cada elo trata a requisição e passa adiante (ex.: repassar o `Authorization` aos subgraphs). |
| Strategy | `SmtpOrderEmailSender.Classify` e o `OrderEmailDispatcher` | O tipo da falha (`Recipient`, `Message`, `Server`) define a reação: falha definitiva, nova tentativa ou pausa da rodada. |
| Session | `IOrderEmailSession` (em `Store/Orders.CommandHandlers/Orders/IOrderEmailSender.cs`) | Uma conexão SMTP por rodada de envio, descartada no fim. |
| Singleton (via DI) | `PaymentGateway` (stub) e `Store/Orders.Infrastructure/Email/EmailDeliveryState.cs` | Estado em memória compartilhado pelo processo. |
| Facade de configuração (extension methods) | `AddInfrastructure`, `AddObservability` (`Frameworker/Frameworker.Observability/ObservabilityExtensions.cs`), endpoints como `app.CreateOrder()` | O `Program.cs` liga cada parte com uma chamada. |

## Programação funcional

| Padrão | Onde | Para quê |
| --- | --- | --- |
| Result (Railway-oriented) | `Frameworker/Programming.Functional/Results/` (`Result`, `Result<T>`); `CreateOrderResult` (`Created`, `Invalid`, `Unavailable`) | Sucesso e falha como valor de retorno, sem exceção para casos esperados. |
| Maybe / Option | `Results/Maybe<T>.cs` e `Options/Option{T}.cs` | Ausência de valor explícita, sem `null`. |

## Resiliência e operação

| Padrão | Onde | Para quê |
| --- | --- | --- |
| Background Worker (polling) | `OrderEmailDispatcher` e `PendingOrderWatcher` (`BackgroundService`) | Envio do e-mail e consulta de pedidos parados fora da requisição. |
| Health Check | `/health/live`, `/health/ready`, `/health/bus`, `/health/ops` e `/health/email`; `StuckOrdersHealthCheck`, `ErrorQueuesHealthCheck` | Prontidão separada da saúde do broker, das filas de erro e do e-mail. |
| Timeout | 5 s na consulta ao Catalog; cliente `fusion` do gateway | Uma dependência lenta não segura a requisição indefinidamente. |
| Optimistic Concurrency | `UsedAt` do `CardToken` (`IsConcurrencyToken()` em `CardTokenConfiguration.cs`) | O mesmo token em duas cobranças simultâneas falha na segunda. |
| Tokenization | `Payments/Payments.Domain/CardToken.cs`, mutation `tokenizeCard` | O cartão vira um `tok_…`; só o token circula entre os serviços. |

## Testes

| Padrão | Onde | Para quê |
| --- | --- | --- |
| BDD (Given/When/Then) | `Tests/Catalogs/Ecommerce.Scenarios.Integration.Spec.Tests` (SpecFlow) | Cenários legíveis contra a API real em memória. |
| Test Doubles (mock, fake) | Moq; `Tests/Payments/Payments.Tests/Fakes/InMemoryPayments.cs`; handler HTTP falso do `Tests/Gateways/Gateway.Tests/GatewayFactory.cs` | Isolar o código testado de banco, broker e outros serviços. |
| Object Mother | `Tests/Gateways/Gateway.Tests/TestTokens.cs` | Tokens prontos para cada situação (válido, expirado, outro audience). |

## O que não é usado

- Pipeline behaviors do MediatR (validação ou log transversal aos handlers).
- Specification pattern.
- Decorator explícito.
- Saga orquestrada (state machine do MassTransit).
- Chave de idempotência no registro do pedido: um novo envio pela loja, depois de uma resposta perdida, cria outro pedido.
