# Proposal

## Why

O Payments cobra no gateway antes de gravar o pagamento. Se a gravação falhar (banco fora, processo derrubado), a mensagem é reentregue e o cartão é cobrado de novo: a proteção contra solicitação repetida só vale depois do registro. Duas entregas simultâneas da mesma solicitação também podem cobrar duas vezes.

## What Changes

- **Payments**:
  - Cada cobrança leva ao gateway uma chave de idempotência que identifica o pedido e a tentativa; a mesma chave nunca gera duas cobranças.
  - O pagamento é registrado como "em processamento" antes de cobrar e recebe o resultado depois.
  - Reentrega de um pagamento em processamento repete a chamada com a mesma chave e recebe o resultado original, em vez de cobrar de novo.
  - Entregas simultâneas da mesma solicitação resultam em uma única cobrança.
- **Gateway simulado**: passa a guardar o resultado por chave e a devolvê-lo nas chamadas repetidas, como os gateways reais.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `payment-processing`: a solicitação repetida passa a ser segura também antes do registro do resultado, e o registro ganha a situação "em processamento".

## Impact

- **Payments**: domínio do pagamento, handler, contrato do gateway, gateway simulado, mapeamento do banco e testes.
- **Banco do Payments**: muda a tabela de pagamentos; como não há migrations, o volume `payment_data` precisa ser recriado.
- **Orders, front-end e mensagens**: sem mudanças.
- **Dependência**: `allow-payment-retry` (nova tentativa com outro cartão) depende desta change, porque usa a tentativa na chave e precisa de mais de um pagamento por pedido.

## Fora do escopo

- Tirar o CVV das mensagens (`tokenize-card-data`).
- Pedido preso em `Pending` quando a mensagem cai na fila de erro (`handle-stuck-payments`).
- Retentativas com intervalo crescente (`add-message-retry-backoff`).
- Persistir as chaves do gateway simulado entre reinícios.
- Nova tentativa de pagamento num pedido recusado (`allow-payment-retry`).
