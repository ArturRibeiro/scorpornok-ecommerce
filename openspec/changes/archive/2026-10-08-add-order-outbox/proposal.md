# Proposal

## Why

Com o RabbitMQ fora do ar, o `POST /createOrder` fica pendurado: o pedido já foi salvo, mas a publicação do `PaymentRequested` espera o broker voltar. Num teste, a requisição passou de 120 s sem resposta. O cliente vê um erro, mantém o carrinho e pode enviar de novo. Quando o broker volta, os dois pedidos são cobrados. Além disso, se o processo cair entre salvar e publicar, o pedido fica `Pending` para sempre (risco já registrado em `add-rabbitmq-payment-flow`).

## What Changes

- **Orders**:
  - A solicitação de pagamento passa a ser gravada no `orders-db` na mesma transação do pedido (outbox transacional do MassTransit com EF Core), em vez de ser publicada direto no broker.
  - Um serviço em segundo plano do próprio `orders-api` entrega as mensagens gravadas ao RabbitMQ e tenta de novo enquanto o broker estiver fora.
  - O `POST /createOrder` responde sem depender do broker, com o mesmo contrato (`201` com o pedido `Pending`).
  - Se a gravação falhar, nem o pedido nem a solicitação ficam registrados.
- **Banco do Orders**: ganha as tabelas do outbox do MassTransit; como não há migrations, o volume do `orders-db` precisa ser recriado.
- **Docs**: `CLAUDE.md` deixa de dizer que o pedido pode ficar `Pending` por falha de publicação; o diagrama `docs/diagrams/order-outbox.excalidraw` passa a fazer parte da documentação.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `order-placement`: o registro do pedido e a solicitação de pagamento passam a ser atômicos e independentes da disponibilidade do broker.

## Impact

- **Pacotes**: `MassTransit.EntityFrameworkCore` 8.5.10 (mesma versão e licença Apache-2.0 do MassTransit atual), no `Orders.Infrastructure`.
- **Orders**: `OrderHandler` (ordem de gravação e publicação, transação explícita), `IOrderRepository`/`OrderRepository`, `OrderContext` (entidades do outbox), `AddInfrastructure` (configuração do outbox), testes do handler.
- **Payments e front-end**: sem mudanças. O Payments já tolera solicitação repetida, e a entrega do outbox é "ao menos uma vez".
- **Dados**: recriar o volume do `orders-db` apaga os pedidos de teste existentes.
- **Dependência**: o delta adiciona requisitos ao `order-placement` e deve ser arquivado depois de `add-checkout-page` e `add-rabbitmq-payment-flow`.

## Fora do escopo

- Outbox no Payments: ele publica dentro do consumidor e, se a publicação falhar, a mensagem volta para a fila e o resultado é republicado.
- Inbox (deduplicação) nos consumidores do Orders: o resultado repetido já não altera um pedido que deixou de ser `Pending`.
- Mudar o `/health/ready`: o MassTransit já registra o check do bus com a tag `ready`, então ele responde `503` sem o broker mesmo com o `POST` funcionando pelo outbox. Separar esse check fica para outra change.
- Alta disponibilidade do RabbitMQ (cluster, quorum queues).
- Migrations do EF Core.
- Remover a perda de mensagem quando a fila do Payments ainda não existe (o `payment-api` continua precisando subir ao menos uma vez).
