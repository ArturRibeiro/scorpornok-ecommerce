# Tasks

## 1. Projeto de observabilidade

- [x] 1.1 Versões de `OpenTelemetry.*`, `Npgsql.OpenTelemetry`, `HotChocolate.Diagnostics` (`$(HotChocolate)`) e `OpenTelemetry.Exporter.InMemory` no `Directory.Build.props`; projeto `Frameworker.Observability` na solution; verificar com `dotnet build Scorponok.slnx`
- [x] 1.2 `AddObservability(serviceName, extraSources)`: recurso (nome, versão, ambiente), traces, métricas e logs, exportador OTLP só com `OTEL_EXPORTER_OTLP_ENDPOINT`; verificar com teste de que sem endpoint não há exportador
- [x] 1.3 Sampler que descarta spans raiz do Npgsql e filtro de `/health` no ASP.NET Core; verificar com testes em `Frameworker.Observability.Tests` (host mínimo com `TestServer`), incluindo `access_token` mascarado na query

## 2. APIs

- [x] 2.1 `AddObservability` no Catalog, Payments e Customers, com a instrumentação do Hot Chocolate sem documento nem variáveis (só tipo e nome da operação); verificar com `dotnet test` dos projetos e `dotnet run` do Catalog sem endpoint respondendo como antes
- [x] 2.2 `AddObservability` no Orders com a fonte `Scorponok.Orders`; `OrderEmailDispatcher` e `PendingOrderWatcher` abrem `Activity` só quando há trabalho; verificar com testes desses serviços (atividade criada só com pedidos) e `dotnet test src/backend/Store/Orders.Tests`
- [x] 2.3 Teste no `Orders.Tests` com exportador em memória: `placeOrder` com CPF, telefone e e-mail sem esses valores em spans e logs
- [x] 2.4 `AddObservability` no gateway; instrumentação do Fusion 16 se existir, senão enricher com tipo e nome da operação; `CardDataTests` conferindo também spans e logs exportados (sucesso e Payments fora); verificar com `dotnet test src/backend/Gateway/Gateway.Tests`

## 3. Compose

- [x] 3.1 `docker-compose.observability.yml`, incluído no `docker-compose.yml`, e `OTEL_EXPORTER_OTLP_ENDPOINT` nas cinco APIs; verificar que `docker compose up -d --build` sem `.env` sobe como antes

## 4. Painel local

- [x] 4.1 Serviço `aspire-dashboard` (tag fixa, perfil `telemetry-local`, interface em 18888, OTLP em 18889) no `docker-compose.observability.yml`; verificar que `docker compose --profile telemetry-local up -d` sobe o painel e que, sem o perfil, ele não sobe
- [x] 4.2 Com o endpoint no painel: uma compra com cartão aparece como um trace do gateway até o resultado no Orders, com logs ligados e serviço, ambiente e versão; registrar se o outbox mantém o trace (senão, ajustar a spec para trace com link); health checks e polling ausentes
- [x] 4.3 Com o painel parado e o endpoint configurado: pedido registrado normalmente
- [x] 4.4 Docs do painel local (`README.md`, `CLAUDE.md`, roteiro de testes e `docs/solucoes-tecnicas/observabilidade.md`)

## 5. Rastrear uma compra

- [x] 5.1 `GraphQLOperationNameProcessor` no `AddObservability`; verificar com teste no `Frameworker.Observability.Tests` (span HTTP renomeado para `mutation PlaceOrder`, e só `query` sem nome) e no `CardDataTests` (`mutation Tokenize` no span `Server`)
- [x] 5.2 `OrderTelemetry.Tag` no `Shared.Code` e `order.number` no Orders (registro, nova tentativa, resultado, consulta do pedido), no Payments e no Customers, com os logs de registro e de resultado; verificar com testes dos handlers (atributo no span corrente)
- [x] 5.3 Span por pedido nas rodadas do `OrderEmailDispatcher` e do `PendingOrderWatcher`; verificar com os testes das rodadas (um span por pedido, com o número)
- [x] 5.4 No painel local: uma compra encontrada pelo filtro `order.number` e listada como `mutation PlaceOrder`; docs (`CLAUDE.md`, `README.md`, roteiro de testes)

## 6. Docs

- [x] 6.1 `CLAUDE.md` (projeto, comandos, compose, cuidados com dados sensíveis), `docs/` e diagrama de arquitetura; `.env` de exemplo nos docs sem chave real
- [x] 6.2 `dotnet test Scorponok.slnx` sem falhas
