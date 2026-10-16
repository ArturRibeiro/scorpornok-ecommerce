# Proposal

## Why

Orders e Payments tentam processar cada mensagem só 4 vezes em cerca de 3 segundos. Uma instabilidade curta (banco reiniciando, rede oscilando) já manda a solicitação ou o resultado do pagamento para a fila de erro, de onde ninguém o tira: o pedido fica pendente para sempre, às vezes com o cartão já cobrado.

## What Changes

- **Orders e Payments**: a política de novas tentativas dos consumidores passa a ter duas fases:
  - poucas tentativas imediatas, para falhas momentâneas;
  - novas tentativas com intervalo crescente, cobrindo até 5 minutos de indisponibilidade.
- Erros que não se resolvem com o tempo (mensagem com dados inválidos) vão direto para a fila de erro, sem esperar os 5 minutos.
- A mensagem continua com o broker até ser processada: reiniciar a API no meio das tentativas não a perde.
- **Docs**: o `CLAUDE.md` passa a descrever a política e o limite de 5 minutos.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `payment-processing`: falhas transitórias no processamento da solicitação não levam à perda nem à fila de erro dentro do limite.
- `order-placement`: o mesmo para a aplicação do resultado do pagamento ao pedido.

## Impact

- **Orders e Payments**: só a configuração do MassTransit nos respectivos `Infrastructure`; handlers e contratos não mudam.
- **Infra**: nenhuma mudança no RabbitMQ.
- **Dependência**: no Payments, repetir o processamento por mais tempo aumenta a janela de cobrança em dobro descrita em `add-payment-idempotency`; esta change deve entrar depois dela.

## Fora do escopo

- Monitorar, alertar e reprocessar a fila de erro, e dar prazo a pedidos pendentes.
- Idempotência da cobrança no gateway (`add-payment-idempotency`).
- Retentativas do envio de e-mail (`limit-email-retries`).
- Plugin de mensagens atrasadas do RabbitMQ ou agendador externo (Quartz/Hangfire).
