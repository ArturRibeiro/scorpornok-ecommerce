# Proposal

## Why

Hoje não há como seguir uma compra entre o gateway, os subgraphs, o RabbitMQ e os serviços em segundo plano: só existem logs no console de cada container e os health checks. Um diagnóstico exige juntar logs de cinco APIs à mão.

## What Changes

- As cinco APIs (gateway, Catalog, Orders, Payments e Customers) passam a emitir traces, métricas e logs num padrão aberto, que qualquer coletor compatível recebe.
- Um trace atravessa a requisição da loja, as chamadas entre serviços, o banco e as mensagens do RabbitMQ, inclusive as que saem pelo outbox.
- Logs ligados ao trace em que foram escritos.
- Traces nomeados pela operação GraphQL e localizáveis pelo número do pedido.
- Serviço, ambiente e versão identificados em toda a telemetria.
- Nenhum dado de cartão, CPF, telefone, e-mail ou token de acesso vai para a telemetria.
- Health checks e rodadas de serviços em segundo plano sem trabalho não geram traces.
- Sem coletor configurado, as APIs funcionam como hoje, sem exportar nada.
- Painel local opcional no compose, sem conta, para ver a telemetria.

## Capabilities

### New Capabilities
- `service-telemetry`: telemetria dos serviços do back-end (traces distribuídos, métricas e logs correlacionados, nomeados pela operação e localizáveis pelo pedido), o que nunca pode constar nela e o funcionamento sem coletor.

### Modified Capabilities
- Nenhuma.

## Impact

- Um projeto compartilhado de observabilidade, usado pelo `Program.cs` de cada API.
- Novos pacotes de instrumentação, com versões centralizadas.
- Compose: um arquivo novo com o painel local e variáveis nas cinco APIs.
- Testes do gateway e das APIs para os dados sensíveis na telemetria.
- Docs (`CLAUDE.md`, `docs/`) e diagrama de arquitetura.

## Fora do escopo

- Monitoramento do front-end (RUM) e ligação da sessão do navegador ao trace.
- Serviço de observabilidade pago (o Datadog foi avaliado e descartado pelo custo; o formato aberto permite adotá-lo depois só com configuração).
- Monitores, dashboards e alertas.
- Métricas de negócio próprias (pedidos por status, e-mails com falha).
- Instrumentação do Keycloak, do RabbitMQ e dos bancos.
- Profiler contínuo e recursos exclusivos de tracers de fornecedor.
- Ambiente de produção real: o alvo é o compose local.
- Guardar a telemetria do painel local entre reinícios.
