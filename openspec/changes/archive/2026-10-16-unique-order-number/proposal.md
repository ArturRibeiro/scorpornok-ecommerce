# Proposal

## Why

O número do pedido vem do hash do objeto em memória, sem garantia de unicidade e sem restrição no banco. Com a chave de idempotência do Payments formada pelo número do pedido e pela tentativa, um número repetido faz um pedido novo ser tratado como solicitação repetida: o cartão não é cobrado e o resultado de um pedido antigo é reaproveitado, o que pode confirmar um pedido sem pagamento. Números repetidos também confundem a tela de confirmação, o acompanhamento em tempo real e o e-mail.

## What Changes

- **Orders**:
  - O número do pedido passa a ser gerado aleatoriamente, com entropia suficiente para não se repetir na prática, mantendo o formato legível (`A` seguido de dígitos).
  - O banco de pedidos passa a recusar dois pedidos com o mesmo número.
  - Uma colisão, se acontecer, falha o registro do pedido sem gravar nem cobrar, em vez de misturar dois pedidos.
- **Docs**: `CLAUDE.md` passa a descrever a geração do número e a restrição de unicidade.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `order-placement`: o número do pedido passa a ser único.

## Impact

- **Orders**: geração do número no domínio do pedido, mapeamento do banco e testes.
- **Banco do Orders**: nova restrição de unicidade; como não há migrations, o volume `orders_data` precisa ser recriado.
- **Payments, front-end e mensagens**: sem mudanças; o número continua sendo texto com `A` e dígitos.
- **Ordem**: implementar e arquivar antes de `add-payment-idempotency`, cuja chave depende de o número ser único.

## Fora do escopo

- Garantir unicidade entre recriações do banco de pedidos com o banco de pagamentos mantido (a entropia do número torna a repetição improvável; a recomendação continua sendo recriar os dois volumes juntos).
- Nova tentativa automática de registro quando houver colisão.
- Números sequenciais ou com significado (data, loja).
