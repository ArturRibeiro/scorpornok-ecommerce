# Tasks

## 1. Domínio

- [x] 1.1 Adicionar ao `OrderPayment` `EmailAttempts`, `NextEmailAttemptAt`, `EmailFailedAt` e `EmailFailureReason`, e ao `Order` `RegisterPaymentEmailRetry` e `MarkPaymentEmailFailed`; ajustar `IsAwaitingPaymentEmail` (sem falha definitiva e próxima tentativa vencida)
- [x] 1.2 Testes em `OrderPaymentTests`: tentativa agenda a próxima, falha definitiva tira o pedido da espera, pedido com tentativa futura não está aguardando; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 2. Classificação no envio

- [x] 2.1 Criar `EmailDeliveryException` com `Kind` (`Recipient`, `Message`, `Server`) e mapear no `SmtpOrderEmailSender` conforme a tabela do design
- [x] 2.2 Testes da classificação com exceções do MailKit simuladas (sem servidor SMTP real); verificar com `dotnet test src/backend/Store/Orders.Tests`

## 3. Dispatcher

- [x] 3.1 `SmtpOptions`: `MaxEmailAttempts` (6), `RetryBaseSeconds` (60), `RetryMaxSeconds` (3600), com validação de valores positivos
- [x] 3.2 `OrderRepository.GetAwaitingPaymentEmailAsync` com o novo critério e a ordenação por `NextEmailAttemptAt`
- [x] 3.3 `OrderEmailDispatcher`: `Recipient` → falha definitiva e segue; `Message` → nova tentativa com intervalo crescente ou falha definitiva no limite, e segue; `Server` → encerra a rodada e dobra a espera até o teto; logs conforme o design
- [x] 3.4 Atualizar `OrderEmailDispatcherTests`: pedido recusado não bloqueia o seguinte, limite de tentativas, intervalos crescentes, `Server` não altera os pedidos e aumenta a espera, recuperação volta ao `PollSeconds`

## 4. Saúde

- [x] 4.1 `EmailDeliveryState` (singleton) e `EmailDeliveryHealthCheck` com a tag `email`; mapear `/health/email` no `Program.cs` do `Orders.Web.Api`, fora do `ready`
- [x] 4.2 Testes do health check: `Unhealthy` após rodada `Server`, `Degraded` com falha definitiva nas últimas 24 h, `Healthy` nos demais casos

## 5. Verificação ponta a ponta e documentação

- [x] 5.1 Recriar o volume do `orders-db` e, com o compose no ar: senha SMTP errada no `.env` deixa `/health/email` em 503 sem gastar tentativas; corrigida, os e-mails saem; pedido com destinatário recusado (Mailpit com `--smtp-reject-recipients` ou e-mail inválido no token de teste) fica em falha definitiva sem bloquear os demais
  - Verificado no compose com um Mailpit temporário (autenticação por arquivo de senhas e `--smtp-allowed-recipients`), sem usar o Gmail: com a senha errada, `/health/email` 503 (motivo `535`), `/health/ready` 200, nenhum pedido com tentativa gasta e o aviso registrado uma vez; com a senha certa, o destinatário recusado (550) e o endereço inválido entraram em falha definitiva na 1ª tentativa sem bloquear os três seguintes, que receberam o e-mail, e `/health/email` ficou 200 "Degraded" com `failedEmails: 2`. Dados de teste removidos e SMTP restaurado para o `.env`. Não exige navegador.
- [x] 5.2 Atualizar o `CLAUDE.md` (e-mail do resultado: limite, intervalo crescente, falha definitiva, `/health/email`, novas chaves em `Smtp`, recriação do volume) e o `Orders.Web.Api.http` com `/health/email`
- [x] 5.3 Rodar `dotnet test Scorponok.slnx`, todos sem falhas

## Workflow follow-up

- Arquivar esta change (`/opsx:archive`) depois da revisão.
