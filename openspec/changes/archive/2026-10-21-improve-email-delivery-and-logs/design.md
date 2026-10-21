# Design

## Context

Motivação em `proposal.md`; requisitos em `specs/`.

- `SmtpOrderEmailSender.SendPaymentResultAsync(order)` cria um `SmtpClient` por e-mail: conecta, autentica, envia e desconecta (`Email/SmtpOrderEmailSender.cs`). Toda falha sai como `EmailDeliveryException` com o tipo (`Recipient`, `Message`, `Server`) dado por `Classify`.
- O `OrderEmailDispatcher` busca até 20 pedidos por rodada, abre um span por pedido (`Envio do e-mail de pagamento`) e age pelo tipo da falha: `Recipient` e `Message` registram no pedido e seguem; `Server` encerra a rodada sem tocar nos pedidos e aumenta a espera.
- No trace medido, o envio de um e-mail levou 2,8 s (aquecido) e 3,3 s (frio), sem nenhum span do SMTP; só apareciam as consultas de revogação do certificado.
- `Frameworker.Observability` já filtra para o coletor o log de cada consulta do EF Core (`Microsoft.EntityFrameworkCore.Database.Command`, abaixo de Warning). Cada chamada HTTP ainda gera quatro logs `Information` (`System.Net.Http.HttpClient.{cliente}.LogicalHandler`/`ClientHandler`), no gateway e no Orders.
- Catalog, Orders, Payments e Customers chamam `UseHttpsRedirection()`; no compose não há HTTPS, e o ASP.NET Core avisa `Failed to determine the https port for redirect` a cada partida. O MediatR 13 avisa a falta de licença (`LuckyPennySoftware.MediatR.License`) no primeiro uso.

## Goals / Non-Goals

**Goals:** uma conexão SMTP por rodada, com as mesmas regras de falha de hoje; etapas do SMTP visíveis no trace; painel sem logs que repetem spans nem avisos de ambiente.

**Non-Goals:** envio paralelo; reaproveitar a conexão entre rodadas; mudar `Classify` ou os limites de tentativa.

## Decisions

1. **Sessão por rodada**: o `IOrderEmailSender` passa a ter `OpenSessionAsync(ct)`, que conecta e autentica e devolve um `IOrderEmailSession` (`SendPaymentResultAsync(order, ct)`, `IAsyncDisposable`, que desconecta com `QUIT`). O dispatcher abre a sessão depois de saber que há pedidos e antes do laço, e a descarta ao fim da rodada. Falha ao abrir é classificada como hoje (conexão, TLS, autenticação → `Server`) e encerra a rodada sem tocar nos pedidos. Falha de um envio segue a regra atual; depois de `Recipient` ou `Message` (respostas 4xx/5xx do servidor), a conexão continua utilizável e o laço segue com ela. Conexão que cai no meio vira `Server` no envio seguinte e encerra a rodada. *Alternativas:* conectar na primeira mensagem dentro da sessão (a conexão apareceria no span do primeiro pedido e a falha de conexão seria atribuída a ele); um `SmtpClient` singleton reaproveitado entre rodadas (conexão ociosa por minutos cai, e o estado entre rodadas complica a classificação).
2. **Spans do SMTP**: na fonte `Scorponok.Orders`, `SMTP conexão` e `SMTP autenticação` (filhos do span da rodada, com `server.address` e `server.port`) e `SMTP envio` (filho do span do pedido, que já tem `order.number`). Em falha, status `Error` e o atributo `email.failure.kind` com o tipo. Nenhum atributo com o destinatário, o assunto ou o corpo. As consultas de revogação do certificado continuam como spans do HttpClient, filhas da conexão.
3. **Logs que repetem spans**: no `AddObservability`, `AddFilter<OpenTelemetryLoggerProvider>("System.Net.Http.HttpClient", LogLevel.Warning)`; o filtro de categoria vale por prefixo, então cobre todos os clientes nomeados e tipados. O console não muda.
4. **Avisos de ambiente**: `UseHttpsRedirection()` sai das quatro APIs: elas só são servidas por HTTP (compose e `dotnet run` nos perfis `http`), e um redirecionamento quebraria o CORS da loja. O aviso de licença do MediatR fica fora do coletor (`LuckyPennySoftware.MediatR.License` em `LogLevel.None` para o `OpenTelemetryLoggerProvider`), e o console continua mostrando. *Alternativa:* `LicenseKey` do MediatR, fora do escopo.
5. **Testes**: um servidor SMTP falso em memória no `Orders.Tests` (`FakeSmtpServer`, sobre `TcpListener`, com respostas roteirizadas: aceita, recusa destinatário com 550, derruba a conexão) para testar o `SmtpOrderEmailSender` de verdade: uma conexão para várias mensagens, recusa de destinatário sem perder a conexão, queda classificada como `Server` e os spans (etapas, status de erro, ausência do destinatário). O dispatcher continua testado com o sender mockado: sessão aberta uma vez por rodada, nenhuma sem pedidos, falha ao abrir sem tocar nos pedidos. Os filtros de log ganham casos no `Frameworker.Observability.Tests`.

## Risks / Trade-offs

- [Servidor encerra a conexão por tempo ou por quantidade de mensagens] → o lote é de 20 e a rodada é curta; uma queda vira `Server`, e a próxima rodada reconecta sem contar tentativa.
- [Estado da conexão depois de uma recusa] → o MailKit reinicia a transação (`RSET`) depois de um comando recusado; o teste com o servidor falso confirma que o envio seguinte passa.
- [Esconder o aviso de licença do painel] → o aviso continua no console; é só ruído para quem olha a telemetria.

## Migration Plan

Sem mudança de dados ou de contrato. Reconstruir as imagens das APIs. Rollback: reverter o commit.
