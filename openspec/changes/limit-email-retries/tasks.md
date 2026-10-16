# Tasks

## 1. Domínio

- [ ] 1.1 Adicionar ao `OrderPayment` `EmailAttempts`, `NextEmailAttemptAt`, `EmailFailedAt` e `EmailFailureReason`, e ao `Order` `RegisterPaymentEmailRetry` e `MarkPaymentEmailFailed`; ajustar `IsAwaitingPaymentEmail` (sem falha definitiva e próxima tentativa vencida)
- [ ] 1.2 Testes em `OrderPaymentTests`: tentativa agenda a próxima, falha definitiva tira o pedido da espera, pedido com tentativa futura não está aguardando; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 2. Classificação no envio

- [ ] 2.1 Criar `EmailDeliveryException` com `Kind` (`Recipient`, `Message`, `Server`) e mapear no `SmtpOrderEmailSender` conforme a tabela do design
- [ ] 2.2 Testes da classificação com exceções do MailKit simuladas (sem servidor SMTP real); verificar com `dotnet test src/backend/Store/Orders.Tests`

## 3. Dispatcher

- [ ] 3.1 `SmtpOptions`: `MaxEmailAttempts` (6), `RetryBaseSeconds` (60), `RetryMaxSeconds` (3600), com validação de valores positivos
- [ ] 3.2 `OrderRepository.GetAwaitingPaymentEmailAsync` com o novo critério e a ordenação por `NextEmailAttemptAt`
- [ ] 3.3 `OrderEmailDispatcher`: `Recipient` → falha definitiva e segue; `Message` → nova tentativa com intervalo crescente ou falha definitiva no limite, e segue; `Server` → encerra a rodada e dobra a espera até o teto; logs conforme o design
- [ ] 3.4 Atualizar `OrderEmailDispatcherTests`: pedido recusado não bloqueia o seguinte, limite de tentativas, intervalos crescentes, `Server` não altera os pedidos e aumenta a espera, recuperação volta ao `PollSeconds`

## 4. Saúde

- [ ] 4.1 `EmailDeliveryState` (singleton) e `EmailDeliveryHealthCheck` com a tag `email`; mapear `/health/email` no `Program.cs` do `Orders.Web.Api`, fora do `ready`
- [ ] 4.2 Testes do health check: `Unhealthy` após rodada `Server`, `Degraded` com falha definitiva nas últimas 24 h, `Healthy` nos demais casos

## 5. Verificação ponta a ponta e documentação

- [ ] 5.1 Recriar o volume do `orders-db` e, com o compose no ar: senha SMTP errada no `.env` deixa `/health/email` em 503 sem gastar tentativas; corrigida, os e-mails saem; pedido com destinatário recusado (Mailpit com `--smtp-reject-recipients` ou e-mail inválido no token de teste) fica em falha definitiva sem bloquear os demais
- [ ] 5.2 Atualizar o `CLAUDE.md` (e-mail do resultado: limite, intervalo crescente, falha definitiva, `/health/email`, novas chaves em `Smtp`, recriação do volume) e o `Orders.Web.Api.http` com `/health/email`
- [ ] 5.3 Rodar `dotnet test Scorponok.slnx`, todos sem falhas

## Workflow follow-up

- Arquivar esta change (`/opsx:archive`) depois da revisão.
