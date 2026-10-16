# Proposal

## Why

Um pedido pode ficar pendente para sempre: falha técnica no Payments esgota as tentativas e a mensagem vai para a fila de erro, ou a solicitação se perde antes de a fila existir. O pior caso é o cliente cobrado com o pedido ainda pendente, porque o resultado aprovado parou na fila de erro do Orders. Em todos eles a tela diz que o resultado chegará por e-mail, e ele nunca chega; ninguém da operação é avisado.

## What Changes

- **Orders**:
  - Pedido pendente além de um prazo configurável é verificado: o Orders pergunta ao Payments qual é a situação do pagamento daquele pedido, em vez de decidir sozinho.
  - Resposta "aprovado" ou "recusado": o pedido é atualizado como hoje (isso também resgata o resultado que tinha parado na fila de erro).
  - Resposta "sem pagamento": o pedido passa para um novo status, **pagamento não processado**, e o cliente recebe um e-mail dizendo que nada foi cobrado e que pode refazer a compra.
  - Resposta "em processamento" ou nenhuma resposta: o pedido continua pendente e é verificado de novo depois.
  - A loja e o e-mail passam a distinguir "pagamento não processado" de "cartão recusado".
- **Payments**:
  - Responde à consulta da situação do pagamento de um pedido.
  - Ao responder "sem pagamento", anula o pedido: uma solicitação que chegue depois para ele é recusada sem cobrança.
- **Operação**:
  - Verificação de saúde que acusa mensagens nas filas de erro e pedidos pendentes além do prazo sem resposta do Payments.
  - Procedimento documentado para reprocessar as filas de erro.

## Capabilities

### New Capabilities
- `payment-flow-monitoring`: visibilidade operacional do fluxo de pagamento (filas de erro e pedidos presos) e o procedimento de reprocessamento.

### Modified Capabilities
- `order-placement`: prazo de verificação do pedido pendente, desfecho "pagamento não processado" e o aviso à loja.
- `order-notification`: e-mail para o pedido com pagamento não processado.
- `payment-processing`: consulta da situação do pagamento e anulação do pedido sem pagamento.

## Impact

- **Orders**: domínio (status novo), um serviço em segundo plano de verificação, consumidores da resposta, e-mail, hub, configuração do prazo, health check.
- **Payments**: consumidor da consulta, anulação, health check.
- **Contratos**: novas mensagens de integração em `Shared.Code`; a mensagem do hub ganha o status do pedido (o campo atual continua).
- **Front-end**: a confirmação do pedido trata o novo desfecho.
- **Banco**: colunas novas no Orders e no Payments; sem migrations, os volumes precisam ser recriados.
- **Dependência**: usa o registro do pagamento "em processamento" antes da cobrança, de `add-payment-idempotency`, para que a consulta e a cobrança não corram em paralelo; deve ser aplicada depois dela.

## Fora do escopo

- Estorno de pagamento: com o Payments como fonte da verdade, o pedido nunca é anulado depois de cobrado.
- Mudar a política de retry dos consumidores (outra change).
- Reprocessamento automático das filas de erro; o procedimento é manual e documentado.
- Alertas fora das verificações de saúde (e-mail para a equipe, Slack, dashboards).
- Nova tentativa de pagamento no mesmo pedido.
