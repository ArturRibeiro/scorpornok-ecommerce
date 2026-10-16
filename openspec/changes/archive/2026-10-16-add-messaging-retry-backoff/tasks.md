# Tasks

## 1. Política de retentativa

- [x] 1.1 Criar `UseTransientRetry` no `Payments.Infrastructure` (exponencial cobrindo cerca de 5 minutos, `Ignore<ArgumentException>()`) e trocar o `UseMessageRetry(Interval(3, 1s))` do `AddInfrastructure` por ela; verificar com `dotnet build Scorponok.slnx`
- [x] 1.2 Fazer o mesmo no `Orders.Infrastructure`, sem mexer no outbox; verificar com `dotnet build Scorponok.slnx`
- [x] 1.3 Somar os intervalos configurados num teste (ou constante comentada) e confirmar que ficam entre 4 e 6 minutos e abaixo do `consumer_timeout` de 30 minutos

## 2. Testes

- [x] 2.1 No `Payments.Tests`, com o test harness: handler que falha duas vezes e depois funciona processa a solicitação sem `Fault`; handler que lança `ArgumentException` gera `Fault` na primeira tentativa (política com intervalos curtos injetados no teste)
- [x] 2.2 Os mesmos dois casos no `Orders.Tests` para o `PaymentApprovedConsumer`; verificar com `dotnet test Scorponok.slnx`

## 3. Verificação ponta a ponta (compose)

- [x] 3.1 Com `docker compose stop payment-db`, criar um pedido; religar o banco depois de cerca de 1 minuto; o pedido termina `Confirmed` e a fila `payment-requested_error` continua vazia (painel do RabbitMQ)
  - Verificado com um pedido `Pending` criado direto no `orders-db` (sem e-mail) e a `PaymentRequested` publicada pela API do RabbitMQ: com o `payment-db` parado por 60 s, o pedido terminou `Confirmed` e a `payment-requested_error` nem chegou a existir. Não foi feito pelo checkout no navegador.
- [x] 3.2 Repetir com `orders-db` parado ao chegar o resultado (parar o banco logo depois do `201`); religar em cerca de 1 minuto; o pedido termina `Confirmed` e `payment-approved_error` continua vazia
  - Verificado com um pedido `Pending` criado direto no `orders-db` e a `PaymentApproved` publicada pela API do RabbitMQ: com o `orders-db` parado por 60 s, o pedido terminou `Confirmed` e a `payment-approved_error` nem chegou a existir. Não foi feito pelo checkout no navegador.
- [x] 3.3 Reinício: com `payment-db` parado, criar um pedido, reiniciar o `payment-api`, religar o banco; o pedido termina `Confirmed`
  - Verificado do mesmo jeito: com o `payment-db` parado, o `payment-api` reiniciado devolveu a mensagem à fila sem perdê-la; o pedido terminou `Confirmed` quando o `payment-api` voltou. Ressalva: o `payment-api` não sobe com o banco fora (o `EnsureCreatedAsync` da partida falha) e o compose não tem política de reinício para as APIs, então foi preciso subi-lo de novo à mão depois de religar o banco. Isso já acontecia antes e fica fora desta change.

## 4. Documentação

- [x] 4.1 Atualizar o `CLAUDE.md` (fluxo do pagamento: retentativa exponencial de cerca de 5 minutos nos consumidores do Orders e do Payments, erros não repetidos, fila de erro depois disso)

## Workflow follow-up

- Implementar depois de `add-payment-idempotency`.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
