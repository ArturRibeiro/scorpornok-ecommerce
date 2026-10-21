# Design

## Context

Motivação em `proposal.md`; requisitos em `specs/service-telemetry/spec.md`.

- Nenhuma API tem telemetria hoje: nem OpenTelemetry, nem Serilog, nem tracer de fornecedor. Os logs vão só para o console.
- Cinco `Program.cs` (gateway, Catalog, Orders, Payments, Customers) configurados à mão, sem projeto compartilhado de setup.
- Caminhos que um trace precisa atravessar: gateway → subgraphs (cliente HTTP `fusion`), Orders → Catalog (`HttpCatalogProducts`), MassTransit sobre RabbitMQ com o outbox do EF no Orders, Npgsql em todas as APIs.
- Serviços que fazem polling: o `OrderEmailDispatcher` (5 s), o `PendingOrderWatcher` (60 s) e a entrega do outbox do MassTransit.
- Dados sensíveis no caminho: o cartão vai **literal no documento** da `tokenizeCard` (não só em variáveis); o hub recebe o token em `access_token` na query; CPF, telefone e e-mail passam pelo `placeOrder` e pelas mensagens. O gateway já desliga `IncludeExceptionDetails` e `CollectOperationPlanTelemetry`, e o `CardDataTests` confere logs e resposta.

## Goals / Non-Goals

**Goals:** instrumentação neutra de fornecedor, configurada num lugar só; dados sensíveis barrados no código e verificados por teste; nada muda para quem roda sem coletor; visualização local, sem conta paga.

**Non-Goals:** mascaramento no coletor como proteção principal; amostragem por volume (todo trace é enviado, o ambiente é local); EF Core instrumentation (o Npgsql já cobre o SQL).

## Decisions

1. **OpenTelemetry SDK + OTLP**, sem tracer de fornecedor. O código não depende de nenhum serviço; trocar o destino (Grafana, Jaeger, um serviço pago) é só apontar o OTLP para outro coletor. *Alternativas:* o Datadog (agente e `dd-trace-dotnet`), avaliado no começo da change e descartado pelo custo (só o trial de 14 dias é gratuito); tracers de fornecedor em geral, que exigem profiler no Dockerfile e dão menos controle sobre o que cada integração grava (o ponto crítico aqui é o cartão).
2. **Projeto `Frameworker.Observability`** com `builder.AddObservability(serviceName, tracing => ...)`, chamado no início de cada `Program.cs` (o callback acrescenta as fontes e instrumentações próprias da API, como a do Hot Chocolate e a `Scorponok.Orders`). Liga:
   - traces: ASP.NET Core, HttpClient, `Npgsql.OpenTelemetry`, a fonte `MassTransit` e, pelo callback, o Hot Chocolate (`HotChocolate.Diagnostics` nos subgraphs, `HotChocolate.Fusion.Diagnostics` no gateway);
   - métricas: ASP.NET Core, HttpClient e runtime;
   - logs: o provedor OpenTelemetry do `ILogger`, com `IncludeFormattedMessage` e sem `IncludeScopes`, ao lado do console.
   Versões dos pacotes `OpenTelemetry.*` e `Npgsql.OpenTelemetry` no `Directory.Build.props`.
3. **Exportação só com endpoint**: o exportador OTLP é registrado só quando `OTEL_EXPORTER_OTLP_ENDPOINT` está definido. Sem ele, nenhum exportador (o pipeline fica, sem custo de rede). Com o coletor fora, o exportador em lote descarta e loga; nenhuma requisição espera o envio.
4. **Identificação**: `service.name` vem do parâmetro (`gateway-api`, `catalog-api`, `orders-api`, `payment-api`, `customers-api`, os nomes do compose), `service.version` da versão informacional do assembly e o ambiente do `ASPNETCORE_ENVIRONMENT`, em `deployment.environment.name` e `deployment.environment` (o nome atual da convenção e o antigo, que muitos coletores ainda leem). `OTEL_SERVICE_NAME`/`OTEL_RESOURCE_ATTRIBUTES` no ambiente sobrescrevem, como no padrão OTel.
5. **GraphQL sem documento nem variáveis**: `GraphQLInstrumentation.WithoutSensitiveData()`, comum ao Hot Chocolate e ao Fusion: `IncludeDocument = false`, `RequestDetails = OperationName` (o padrão do Hot Chocolate 16 grava as variáveis e, com elas, o corpo da requisição) e `MaxErrorEvents = 0` (a mensagem de um erro pode repetir um valor do documento). Necessário porque o cartão está no texto da `tokenizeCard`, não só em variáveis. Nenhum enricher grava argumentos ou corpo.
6. **Query e cabeçalhos**: a instrumentação de ASP.NET Core e de HttpClient já mascara os valores da query string e não grava cabeçalhos; não se liga `RecordException` com payload nem enrichers de corpo. Um teste confere o `access_token` do hub.
7. **Polling sem trace**: um processor (`DropRootDatabaseSpansProcessor`, antes do exportador) tira a marca `Recorded` dos spans raiz (sem pai) do Npgsql; o sampler não serve porque não recebe a fonte do span. Isso cobre as consultas de polling, a entrega do outbox e o `EnsureCreated`/seed na partida. O `OrderEmailDispatcher` e o `PendingOrderWatcher` abrem uma `Activity` da fonte `Scorponok.Orders` só quando a consulta devolve trabalho, e o processamento segue dentro dela. Health checks são filtrados na instrumentação do ASP.NET Core pelo caminho `/health`. *Alternativa:* `SuppressInstrumentationScope` em cada consulta de polling: espalha detalhe de telemetria pelo código de negócio e não alcança o outbox do MassTransit.
8. **Painel local: .NET Aspire Dashboard**, em `docker-compose.observability.yml` (incluído pelo `docker-compose.yml`), no perfil `telemetry-local` (`mcr.microsoft.com/dotnet/aspire-dashboard`, tag fixa): recebe OTLP gRPC em 18889 e mostra traces, métricas e logs em http://localhost:18888, sem conta, com os dados em memória. Anônimo (`DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS`), aceitável só porque roda local. As APIs recebem `OTEL_EXPORTER_OTLP_ENDPOINT: ${OTEL_EXPORTER_OTLP_ENDPOINT:-}`: sem o perfil e sem a variável, o compose sobe como antes. Para usar: `OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889` e `docker compose --profile telemetry-local up -d`. *Alternativas:* Grafana LGTM (guarda os dados e tem dashboards, mas é mais pesado e disputaria portas com a loja), Jaeger (só traces), SigNoz (vários containers e conflita com a porta do Keycloak).
9. **Nome do trace = operação GraphQL**: o `GraphQLOperationNameProcessor` (em `Frameworker.Observability`, sem depender do Hot Chocolate) observa o fim do span da operação (o de `graphql.processing.type = request`, com `graphql.operation.type` e `graphql.operation.name`), guarda `"{tipo} {nome}"` no span `Server` do ASP.NET Core mais próximo acima dele (custom property) e, no fim desse span, troca o `DisplayName`. O fim do span HTTP vem depois do da operação e depois de a instrumentação do ASP.NET Core dar o nome pela rota, então a troca prevalece. Vale no gateway e nos subgraphs (no subgraph, o nome é o que o Fusion gera, ex.: `PlaceOrder_e68973cb_1`). Os atributos HTTP (`http.route`, `url.path`) continuam. *Alternativa:* um `ActivityEnricher` do Hot Chocolate e outro do Fusion: duas classes com a mesma regra, presas a APIs internas de cada pacote.
10. **Número do pedido nos spans**: atributo `order.number`, gravado pelo `OrderTelemetry.Tag(orderNumber)` do `Shared.Code` (só `System.Diagnostics`: os handlers não dependem do OpenTelemetry) no span corrente, nos pontos onde o número é conhecido: registro (`OrderHandler`), nova tentativa, aplicação do resultado (`OrderPaymentHandlers`), consulta do pedido do cliente, Payments (`ProcessPaymentHandler`, `CheckPaymentStatusHandler`) e Customers (`RecordPurchaseHandler`). Nos serviços em segundo plano, cada pedido da rodada ganha um span filho (`Envio do e-mail de pagamento`, `Consulta do pagamento`) com o número. Logs `Pedido {OrderNumber} registrado.` e `Pedido {OrderNumber}: pagamento {Status} (tentativa {Attempt}).`, ambos no Orders, em `Information` (o Payments fica sem log novo: o número já está nos spans dele, e o resultado é registrado onde é aplicado). O número não é dado sensível (vai no e-mail e na URL da loja).
11. **Testes de dados sensíveis** com o exportador em memória (`OpenTelemetry.Exporter.InMemory`), adicionado pelos testes via `ConfigureOpenTelemetryTracerProvider`/`LoggerProvider`:
   - `Gateway.Tests`: o `CardDataTests` passa a conferir também spans e logs exportados (sucesso e Payments fora);
   - `Orders.Tests`: `placeOrder` com CPF, telefone e e-mail, executado no Hot Chocolate com a instrumentação ligada, sem esses valores nos spans e logs;
   - `Frameworker.Observability.Tests` (host mínimo com `TestServer`): sem endpoint não há exportador; `access_token` na query não aparece no span; spans de `/health` e raiz do Npgsql descartados.

## Risks / Trade-offs

- [Outbox perde o contexto] O trace só continua depois do outbox se o MassTransit gravar o `traceparent` na mensagem do outbox → verificado no painel local: grava, e a compra inteira (gateway, Orders, Catalog, outbox, Customers, Payments e o resultado no Orders) fica num trace só; a spec fica como está.
- [Fusion 16 sem instrumentação própria] → o gateway fica com os spans de ASP.NET Core e HttpClient, nomeados pela operação via enricher que lê só tipo e nome; os subgraphs têm os spans do Hot Chocolate.
- [Instrumentação futura passar a gravar o corpo ou o documento] → os testes com exportador em memória falham.
- [Painel sem persistência nem login] → é para desenvolvimento local; os dados somem ao reiniciá-lo e a interface só é publicada na máquina (porta 18888), com o OTLP só na rede interna.
- [Spans de banco descartados quando não há pai] também some uma consulta raiz legítima fora de requisição → aceitável: todo trabalho real ou está numa requisição, numa mensagem ou numa `Activity` aberta pelo serviço em segundo plano.

## Migration Plan

Sem mudança de dados ou de contrato. Reconstruir as imagens das cinco APIs. Para ver a telemetria, subir com o perfil `telemetry-local` e o endereço do painel. Rollback: remover o endpoint do `.env` (nada é exportado) ou reverter o commit.
