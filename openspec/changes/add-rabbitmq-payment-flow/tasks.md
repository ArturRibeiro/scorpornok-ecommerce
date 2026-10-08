# Tasks

## 1. Base: broker, pacotes e contratos

- [x] 1.1 Criar `docker-compose.messaging.yml` com `rabbitmq:4.1-management` (5672, 15672, healthcheck) e incluí-lo no `docker-compose.yml`; verificar com `docker compose -f docker-compose.messaging.yml up -d` ficando `healthy` e o painel respondendo em `http://localhost:15672`
- [x] 1.2 Adicionar `$(MassTransit)` = 8.5.10 ao `Directory.Build.props`; verificar com `dotnet build Scorponok.sln`
- [x] 1.3 Criar em `Shared.Code` os contratos `PaymentRequested`/`CardData`/`PaymentApproved`/`PaymentRejected` (`Shared.Code.IntegrationEvents`) e a interface `IIntegrationEventPublisher`; verificar com `dotnet build Scorponok.sln`

## 2. Payments: domínio e processamento

- [x] 2.1 Criar o projeto de testes `src/backend/Payments/Gateway.Payment.Tests` (NUnit + FluentAssertions + Moq) na pasta "3 - Payments" da solution; verificar com `dotnet test src/backend/Payments/Gateway.Payment.Tests` rodando sem erro
- [x] 2.2 Substituir `PaymentMethod` por `Payment` (`Entity<Guid>`, `IAggregateRoot`, só os 4 últimos dígitos, sem CVV), ajustar `PaymentResult` para `(Approved, Reason)` e criar `IPaymentRepository`; verificar com testes da fábrica do `Payment` (dígitos finais, ausência de CVV e do número completo)
- [x] 2.3 Implementar a regra do `PaymentGateway` (final `0000` recusa, demais aprovam); verificar com testes dos dois casos
- [x] 2.4 Criar `Gateway.Payment.CommandHandlers` com `ProcessPaymentCommand` e handler (processa, grava, publica aprovado/recusado; com pagamento já registrado, só republica); verificar com testes do handler com repositório, gateway e publicador mockados: aprovado, recusado e solicitação repetida (nenhum `Add`, mesmo `PaymentId` republicado)

## 3. Payments: infraestrutura e API

- [x] 3.1 Criar em `Gateway.Payment.Data` o `PaymentContext` (Npgsql, schema `payment`), `PaymentConfiguration` (no lugar de `CreditCardPaymentConfiguration`, índice único em `OrderNumber`) e `PaymentRepository`; verificar com `dotnet build Scorponok.sln`
- [x] 3.2 Criar `PaymentRequestedConsumer`, o publicador MassTransit e `AddInfrastructure` (DbContext, repositório, gateway, MediatR, MassTransit com o consumidor); verificar com teste do consumidor no `MassTransit.Testing` harness enviando `ProcessPaymentCommand` ao receber `PaymentRequested`
- [x] 3.3 Trocar o template do `Gateway.Payment.Web.Api` pela API real (connection string obrigatória, `RabbitMq` no `appsettings.Development.json`, `EnsureCreatedAsync`, health `ready` com o banco, `public partial class Program`); verificar subindo com `dotnet run` contra o `payment-db` e o RabbitMQ: `/health/ready` 200, tabela `payment.Payments` criada e fila do consumidor visível no painel

## 4. Orders: solicitação, resultado e aviso à loja

- [x] 4.1 Adicionar `Order.ConfirmPayment(Guid)` e `Order.FailPayment()` (só a partir de `Pending`) e `IOrderRepository.GetByNumberAsync`; verificar com testes de domínio das transições e da imutabilidade fora de `Pending`
- [x] 4.2 Publicar `PaymentRequested` no `OrderHandler` depois de salvar (valor = `order.Total`; falha de publicação só registra log); verificar com testes: válido publica com o total do pedido e ignora `card.amount`; inválido não publica; exceção do publicador ainda devolve sucesso
- [x] 4.3 Criar `IOrderPaymentNotifier`, `ConfirmOrderPaymentCommand`, `FailOrderPaymentCommand` e `GetOrderStatusQuery` com handlers; verificar com testes: confirma com `PaymentId` e notifica aprovado, falha sem `PaymentId` e notifica recusado, ignora pedido não pendente ou inexistente sem notificar, consulta de status devolve `null` para número desconhecido
- [x] 4.4 Criar os consumidores `PaymentApprovedConsumer`/`PaymentRejectedConsumer`, o publicador MassTransit e a configuração do MassTransit no `AddInfrastructure`, com `RabbitMq` no `appsettings.Development.json`; verificar com teste dos consumidores no harness enviando os comandos certos
- [x] 4.5 Criar o `OrderHub` (`/hubs/orders`, `WatchOrder`) e o `SignalROrderPaymentNotifier`, e trocar o CORS do Orders para `AllowCredentials()`; verificar com `dotnet run` e um cliente SignalR de teste (script no scratchpad): recebe `PaymentStatusChanged` ao confirmar um pedido acompanhado, recebe na hora ao acompanhar um pedido já resolvido, e a origem `http://exemplo.com` é recusada

## 5. Compose e documentação

- [x] 5.1 Passar `RabbitMq__Host` para `orders-api` e `payment-api` no `docker-compose.yml`, com `depends_on` do `rabbitmq` saudável, e tirar o comentário de "ainda não é lida" da connection string do Payments; verificar com `docker compose up -d --build` e as três APIs respondendo `/health/ready`
- [x] 5.2 Fluxo ponta a ponta pelas APIs: `POST /createOrder` com cartão final `1111` termina `Confirmed` com `PaymentId`, final `0000` termina `Failed`; conferir os registros em `payment.Payments` (sem CVV, só os 4 dígitos) e reenviar uma mensagem pelo painel para confirmar que não há segundo pagamento
- [x] 5.3 Parar o `payment-api`, criar um pedido, subir de novo e verificar que o pedido sai de `Pending`
- [x] 5.4 Atualizar o `CLAUDE.md` da raiz (RabbitMQ, mensagens, fluxo, hub `/hubs/orders`, CORS com credenciais, projetos do Payments, comando para subir o broker com `dotnet run`); verificar que os comandos citados rodam como descritos

## 6. Front-end

- [x] 6.1 Instalar `@microsoft/signalr` 10.0.11 e criar `lib/orderHub.ts` (`watchOrderPayment`, encerra a conexão ao receber o resultado ou ao abortar); verificar com `npm run build` e `npm run lint`
- [x] 6.2 Fazer o `OrderConfirmation` esperar a mensagem do hub (tempo-limite de 30 s) e mostrar processando/aprovado/recusado/ainda em processamento; verificar no navegador os casos aprovado (final `1111`), recusado (final `0000`) e sem resposta (com o `payment-api` parado), e na aba Network que não há requisições repetidas ao Orders além da conexão do hub
- [x] 6.3 Atualizar o `src/frontend/CLAUDE.md` (conexão com o hub e acompanhamento do pagamento); verificar que a descrição bate com o código

## 7. Integração

- [x] 7.1 Rodar `dotnet test Scorponok.sln` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas

## Workflow follow-up

- Arquivar a change `add-checkout-page` antes desta, porque esta modifica requisitos que ela criou.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
