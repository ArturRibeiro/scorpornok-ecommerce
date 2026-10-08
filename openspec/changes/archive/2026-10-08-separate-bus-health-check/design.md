# Design

## Context

- `Orders.Web.Api/Program.cs` registra `AddHealthChecks().AddDbContextCheck<OrderContext>(tags: ["ready"])` e mapeia `/health/live` (sem checks) e `/health/ready` (checks com a tag `ready`).
- O `AddMassTransit` do `Orders.Infrastructure` (`AddInfrastructure`) registra por conta própria o check do bus, com as tags padrão `ready` e `masstransit`. É ele que leva o `/health/ready` a `503` sem o RabbitMQ.
- A motivação está em `proposal.md`; o comportamento esperado, na spec delta de `order-placement`.

## Goals / Non-Goals

**Goals:**
- O check do bus continua existindo e reportando o estado real do broker, só que fora do `ready`.

**Non-Goals:**
- Mudar o comportamento do check do bus (nome, `FailureStatus`, `MinimalFailureStatus`).

## Decisions

### 1. Trocar as tags do check do bus com `ConfigureHealthCheckOptions`
No `AddMassTransit` do Orders: `x.ConfigureHealthCheckOptions(o => { o.Tags.Clear(); o.Tags.Add("bus"); })`. O check sai do `ready` porque deixa de ter essa tag, sem remover o registro.

- *Alternativa:* filtrar por nome no predicate do `/health/ready` (`check.Tags.Contains("ready") && check.Name != "masstransit-bus"`). Depende do nome interno do check e deixa a tag `ready` mentindo sobre o check.
- *Alternativa:* `MinimalFailureStatus = Degraded` mantendo a tag. O `ready` passaria a responder `200` com "Degraded", mas o filtro de quem consome o endpoint teria de entender esse status; as tags são mais claras.

### 2. Endpoint `/health/bus`
`app.MapHealthChecks("/health/bus", ...)` no `Program.cs`, junto dos outros dois, com predicate pela tag `bus` e `ResultStatusCodes` mapeando `Degraded` para `503`. Mesmo formato de resposta (texto `Healthy`/`Unhealthy`/`Degraded`).

- Na implementação vimos que, quando o broker cai depois da partida, o MassTransit reporta o bus como `Degraded` (reconectando), não `Unhealthy`; só uma partida sem broker dá `Unhealthy`. Com o mapeamento padrão (`Degraded` → `200`) o `/health/bus` esconderia a queda, por isso o `Degraded` vira `503` nesse endpoint. Efeito colateral: durante a partida, enquanto o bus sobe, o `/health/bus` pode responder `503` por alguns segundos, aceitável para monitoramento.
- *Alternativa:* `MinimalFailureStatus`/`FailureStatus` nas opções do MassTransit — muda o status do próprio check, e não só a resposta deste endpoint; o mapeamento no endpoint deixa o check como o MassTransit o define.

### 3. Só o Orders
O Payments mantém o padrão do MassTransit (ver "Fora do escopo" na proposal). A tag `bus` fica restrita ao Orders.

## Risks / Trade-offs

- [Atualização do MassTransit mudar as tags padrão ou a API] → a configuração é explícita (`Tags.Clear()` + `bus`), e a verificação ponta a ponta com o broker parado cobre a regressão.
- [Quem quiser "tudo funcionando" no ready perde essa informação] → ela continua no `/health/bus`.

## Migration Plan

1. Implantar o `orders-api`; nada no compose consome os endpoints de health das APIs.
2. Rollback: voltar o código; o `ready` volta a incluir o bus.
