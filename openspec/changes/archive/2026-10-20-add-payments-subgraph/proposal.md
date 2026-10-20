# Proposal

## Why

A loja ainda chama o Payments direto para tokenizar o cartão; é a única chamada de negócio fora do ponto de entrada GraphQL, além do acompanhamento do pagamento. Passar a tokenização pelo gateway deixa a loja com um único ponto de entrada, ao custo de o cartão atravessar o gateway, o que exige proteções explícitas.

## What Changes

- Payments como subgraph: tokenizar o cartão, com as mesmas regras e os mesmos erros por campo de hoje.
- O checkout e a nova tentativa tokenizam pelo gateway; nada muda na tela.
- O gateway não registra nem devolve os dados do cartão em log, telemetria ou erros.
- REST `/card-tokens` continua por enquanto.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `checkout`: o cartão vai ao serviço de tokenização pelo ponto de entrada, sem ficar registrado no caminho.
- `graphql-gateway`: tokenização pelo ponto de entrada, sem expor os dados do cartão.

## Impact

- Payments: schema GraphQL ao lado do REST.
- Gateway: composição com o Payments e proteções de log.
- Front-end: tokenização pelo gateway; sai a URL do Payments.
- Docs e diagrama de arquitetura.

## Fora do escopo

- Exigir login na tokenização (continua como hoje).
- Remover o REST `/card-tokens`.
- Subscription no lugar do SignalR.
