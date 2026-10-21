# Tasks

## 1. Sessão SMTP por rodada

- [x] 1.1 `FakeSmtpServer` no `Orders.Tests` (TcpListener, respostas roteirizadas: aceita, 550 no destinatário, queda da conexão) com contagem de conexões; verificar com um teste que o MailKit conecta e envia por ele
- [x] 1.2 `IOrderEmailSender.OpenSessionAsync` e `IOrderEmailSession` no `SmtpOrderEmailSender` (conecta e autentica uma vez; desconecta no dispose; mesma classificação de falhas); verificar com testes no servidor falso: três mensagens numa conexão, 550 sem perder a conexão, queda como `Server`
- [x] 1.3 `OrderEmailDispatcher` abrindo a sessão depois de saber que há pedidos e antes do laço, com a falha ao abrir tratada como servidor fora; atualizar os testes do dispatcher (sessão uma vez por rodada, nenhuma sem pedidos, falha ao abrir sem tocar nos pedidos, recusa no meio seguindo com a mesma sessão) e verificar com `dotnet test src/backend/Store/Orders.Tests`

## 2. Telemetria do envio

- [x] 2.1 Spans `SMTP conexão`, `SMTP autenticação` (na rodada) e `SMTP envio` (no pedido), com erro e `email.failure.kind`; verificar com testes no servidor falso (etapas e durações, status de erro na conexão recusada, destinatário ausente de todos os spans)

## 3. Logs

- [x] 3.1 Filtros no `AddObservability`: `System.Net.Http.HttpClient` abaixo de Warning e `LuckyPennySoftware.MediatR.License` fora do coletor; verificar com testes no `Frameworker.Observability.Tests` (log Information de cliente HTTP não exportado, Warning exportado, aviso de licença não exportado)
- [x] 3.2 Tirar `UseHttpsRedirection()` do Catalog, Orders, Payments e Customers; verificar com `docker compose up -d --build` que o aviso de porta HTTPS não aparece no `docker compose logs` e que os `/health/ready` respondem 200

## 4. Verificação e docs

- [x] 4.1 No compose com o painel: dois pedidos e uma rodada de e-mail com uma conexão, uma autenticação e dois envios no trace, sem o destinatário; nenhum log `System.Net.Http.HttpClient` Information nem aviso de licença no painel; comparar a duração do envio com os 2,8 s medidos antes
- [x] 4.2 Docs: `CLAUDE.md` (envio de e-mail e telemetria), README (Observabilidade) e `docs/solucoes-tecnicas/orders.md`/`observabilidade.md`; `dotnet test Scorponok.slnx` sem falhas
