# Tasks

## 1. Política de retentativa

- [ ] 1.1 Criar `UseTransientRetry` no `Payments.Infrastructure` (exponencial cobrindo cerca de 5 minutos, `Ignore<ArgumentException>()`) e trocar o `UseMessageRetry(Interval(3, 1s))` do `AddInfrastructure` por ela; verificar com `dotnet build Scorponok.slnx`
- [ ] 1.2 Fazer o mesmo no `Orders.Infrastructure`, sem mexer no outbox; verificar com `dotnet build Scorponok.slnx`
- [ ] 1.3 Somar os intervalos configurados num teste (ou constante comentada) e confirmar que ficam entre 4 e 6 minutos e abaixo do `consumer_timeout` de 30 minutos

## 2. Testes

- [ ] 2.1 No `Payments.Tests`, com o test harness: handler que falha duas vezes e depois funciona processa a solicitação sem `Fault`; handler que lança `ArgumentException` gera `Fault` na primeira tentativa (política com intervalos curtos injetados no teste)
- [ ] 2.2 Os mesmos dois casos no `Orders.Tests` para o `PaymentApprovedConsumer`; verificar com `dotnet test Scorponok.slnx`

## 3. Verificação ponta a ponta (compose)

- [ ] 3.1 Com `docker compose stop payment-db`, criar um pedido; religar o banco depois de cerca de 1 minuto; o pedido termina `Confirmed` e a fila `payment-requested_error` continua vazia (painel do RabbitMQ)
- [ ] 3.2 Repetir com `orders-db` parado ao chegar o resultado (parar o banco logo depois do `201`); religar em cerca de 1 minuto; o pedido termina `Confirmed` e `payment-approved_error` continua vazia
- [ ] 3.3 Reinício: com `payment-db` parado, criar um pedido, reiniciar o `payment-api`, religar o banco; o pedido termina `Confirmed`

## 4. Documentação

- [ ] 4.1 Atualizar o `CLAUDE.md` (fluxo do pagamento: retentativa exponencial de cerca de 5 minutos nos consumidores do Orders e do Payments, erros não repetidos, fila de erro depois disso)

## Workflow follow-up

- Implementar depois de `add-payment-idempotency`.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
