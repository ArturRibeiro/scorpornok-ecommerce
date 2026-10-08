# Tasks

## 1. Health checks do Orders

- [x] 1.1 No `AddMassTransit` do `Orders.Infrastructure`, configurar `ConfigureHealthCheckOptions` com as tags trocadas para só `bus`; mapear `/health/bus` (predicate pela tag `bus`) no `Orders.Web.Api/Program.cs` e atualizar o comentário dos endpoints; verificar com `dotnet build Scorponok.sln` e, no compose com tudo no ar (`docker compose up -d --build orders-api`), `curl` em `/health/ready` e `/health/bus` do Orders respondendo `200`
- [x] 1.2 Broker parado (`docker compose stop rabbitmq`): `/health/ready` do Orders responde `200`, `/health/bus` responde `503` e um `POST /createOrder` válido responde `201`; com o broker de volta, `/health/bus` volta a `200` e o pedido termina `Confirmed`
- [x] 1.3 Banco parado (`docker compose stop orders-db`): `/health/ready` do Orders responde `503`; religar o banco depois
- [x] 1.4 Payments sem mudança (padrão do MassTransit): com o broker caindo depois da partida, `/health/ready` do `payment-api` responde `200` (bus `Degraded`); reiniciado sem o broker, responde `503` (bus `Unhealthy`)

## 2. Documentação e integração

- [x] 2.1 Atualizar a linha de health checks do `CLAUDE.md` da raiz: o `ready` do Orders só com o banco, o `/health/bus` do Orders com o estado do broker e o `ready` do Payments ainda com o bus; verificar que o texto bate com o código
- [x] 2.2 Rodar `dotnet test Scorponok.sln`, todos sem falhas

## Workflow follow-up

- Arquivar esta change (`/opsx:archive`) depois da revisão.
