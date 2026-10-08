# Proposal

## Why

O MassTransit registra sozinho um health check do bus com a tag `ready`, então o `/health/ready` do Orders responde `503` quando o RabbitMQ está fora, mesmo com o `POST /createOrder` funcionando pelo outbox. Um orquestrador ou balanceador que use esse endpoint tiraria todas as instâncias do tráfego numa queda do broker, desfazendo o que o outbox garante. Hoje nada consome o endpoint (o compose só checa bancos e RabbitMQ), então é o momento barato de corrigir, antes de haver quem dependa dele.

## What Changes

- O `/health/ready` do Orders passa a refletir só o que impede registrar pedidos: o banco do Orders. O estado do broker deixa de afetá-lo.
- Novo endpoint `/health/bus` no Orders, que responde `200` com o broker no ar e `503` sem ele, para monitoramento e alertas.
- `/health/live` não muda.
- O `CLAUDE.md` da raiz deixa de descrever o `503` do `ready` sem o broker e passa a descrever o `/health/bus`.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `order-placement`: novo requisito sobre a prontidão da API de Orders, que não depende do broker, e sobre o estado do broker exposto em separado.

## Impact

- `Orders.Infrastructure` (opções do health check do MassTransit) e `Orders.Web.Api/Program.cs` (mapeamento do `/health/bus`).
- `CLAUDE.md` da raiz (seção de health checks).
- Quem passar a monitorar o broker deve usar o `/health/bus`, não o `/health/ready`.

## Fora do escopo

- Payments: ele não recebe requisições de negócio por HTTP (só consome mensagens), então tirá-lo do tráfego não tem efeito, e sem broker ele de fato não faz nada. O `ready` dele continua incluindo o bus.
- Catalog: não usa o MassTransit.
- Health checks no `docker-compose` para as APIs (`healthcheck`/`service_healthy`).
- Check do servidor SMTP: uma falha de e-mail já não afeta o pedido.
- Formato detalhado (JSON com cada check) nas respostas de health.
