# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- `Orders.Infrastructure` e `Payments.Infrastructure` configuram, no `UsingRabbitMq`, `cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)))` para todos os endpoints, seguido de `ConfigureEndpoints`. Esgotadas as tentativas, o MassTransit move a mensagem para `<fila>_error`.
- Consumidores: `PaymentRequestedConsumer` (Payments) e `PaymentApprovedConsumer`/`PaymentRejectedConsumer` (Orders). Todos só repassam um comando ao handler pelo `IMemoryBus`.
- Os handlers já toleram reprocessamento: o Payments republica o resultado se o pedido já tem pagamento; o Orders ignora resultado para pedido que não está mais `Pending`.
- O Orders usa o bus outbox (`AddEntityFrameworkOutbox` + `UseBusOutbox`) só para publicar a partir da requisição HTTP; os consumidores não publicam.
- O broker é `rabbitmq:4.1-management`, sem plugins extras. O RabbitMQ fecha o canal de um consumidor que segura uma mensagem sem confirmar além do `consumer_timeout` (30 minutos por padrão).

## Goals / Non-Goals

**Goals:**
- Cobrir até 5 minutos de falha transitória sem a mensagem ir para a fila de erro.
- Não perder mensagem se a API reiniciar no meio das tentativas.
- Mesma política nos dois serviços, num único lugar de cada um.

**Non-Goals:**
- Mudar handlers, contratos ou filas.
- Tratar o que acontece depois da fila de erro.

## Decisions

### 1. Retentativas em processo com intervalo exponencial
```csharp
cfg.UseMessageRetry(r =>
{
    r.Ignore<ArgumentException>();
    r.Exponential(8, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(2), TimeSpan.FromSeconds(4));
});
```
A mensagem fica com o consumidor, sem confirmação, enquanto ele espera: se o processo cair, o broker a devolve à fila. A soma dos intervalos (cerca de 1 s, 5 s, 9 s… limitados a 2 min) fica perto de 5 minutos, bem abaixo do `consumer_timeout`. Os parâmetros exatos são ajustados na implementação para fechar perto de 5 minutos.

- *Alternativa:* `UseDelayedRedelivery`: devolve a mensagem ao broker com atraso e libera o consumidor, mas exige o plugin `rabbitmq_delayed_message_exchange` (imagem própria do RabbitMQ, plugin da comunidade com limitações conhecidas de escala). Desproporcional para dois consumidores.
- *Alternativa:* `UseScheduledRedelivery` com Quartz ou Hangfire: exige um agendador persistente (mais tabelas ou serviço); em memória, a mensagem agendada se perde num reinício, o que viola um dos objetivos.

### 2. Duas fases numa política só
O `Exponential` já começa com intervalos curtos, cobrindo as falhas momentâneas que o `Interval(3, 1s)` cobria; não há um `Immediate` separado.

### 3. Erros que não se repetem
`ArgumentException` (e derivadas, como `ArgumentNullException`) vai direto para a fila de erro: indica mensagem malformada, que não muda com o tempo. Falhas de desserialização já vão para a fila de erro pelo próprio MassTransit, antes do retry. A lista fica num método de extensão compartilhado para os dois serviços acrescentarem tipos no futuro.

### 4. Onde fica
Um método `UseTransientRetry(this IRabbitMqBusFactoryConfigurator)` em cada `Infrastructure` (os serviços não compartilham projeto de infraestrutura de mensageria). O `Shared.Code` não referencia o MassTransit e assim continua.

## Risks / Trade-offs

- [Durante uma falha longa, cada mensagem ocupa uma vaga de concorrência do consumidor por até 5 minutos] → numa indisponibilidade do banco, todas as mensagens falhariam de qualquer forma; o volume do projeto é baixo.
- [No Payments, repetir por mais tempo amplia a janela de cobrança em dobro (gateway cobra, gravação falha, nova tentativa cobra de novo)] → depende de `add-payment-idempotency`, que deve entrar antes.
- [Mensagens fora de ordem: um resultado em retentativa pode ser ultrapassado por outro] → o Orders só aceita o primeiro resultado de um pedido pendente; resultados do mesmo pedido são o mesmo resultado republicado.
- [Testar 5 minutos é lento] → os testes de unidade usam o test harness com a política parametrizada; a verificação ponta a ponta mede uma falha de cerca de 1 minuto.

## Migration Plan

- Sem mudança de banco, fila ou mensagem. Basta publicar as duas APIs.
- Rollback: reverter o commit; mensagens em retentativa no momento voltam à fila e são reprocessadas com a política antiga.
