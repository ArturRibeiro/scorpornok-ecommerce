# Proposal

## Why

O cartão completo, inclusive o CVV, vai do navegador ao Orders e segue para o Payments na mensagem de solicitação de pagamento. Ele fica gravado no banco do Orders até a entrega, na fila do RabbitMQ até o consumo e, depois de qualquer falha, na fila de erro por tempo indeterminado. Guardar o CVV depois da autorização é proibido pelo PCI DSS, e o Orders nem precisa ver o cartão.

## What Changes

- **Payments**: novo serviço de tokenização, que simula o cofre de um gateway real. O navegador envia o cartão direto a ele e recebe um token opaco, de uso único e validade curta. O CVV é conferido e descartado; o número completo não é gravado.
- **Front-end**: o checkout tokeniza o cartão antes de criar o pedido e envia ao Orders só o token e as parcelas.
- **Orders**: o pedido recebe só o token; a solicitação de pagamento leva o token no lugar do cartão. **BREAKING**: o contrato do registro do pedido e o da mensagem mudam juntos.
- **Payments**: a cobrança usa o token. Token vencido, já usado ou desconhecido recusa o pagamento com motivo claro, e o pedido termina com falha no pagamento.
- **Dados existentes**: as filas de solicitação de pagamento (inclusive a de erro) são esvaziadas e os volumes dos bancos são recriados, para não sobrar cartão gravado.
- **Docs**: `CLAUDE.md` deixa de dizer que o cartão e o CVV ficam no banco do Orders e nas filas.

## Capabilities

### New Capabilities
- Nenhuma.

### Modified Capabilities
- `checkout`: o cartão vai só para a tokenização; ao serviço de pedidos vai o token.
- `order-placement`: o pedido e a solicitação de pagamento carregam o token, nunca dados do cartão.
- `payment-processing`: tokenização do cartão, cobrança pelo token e recusa de token inválido.

## Impact

- **Payments**: passa a expor um endpoint chamado pelo navegador (CORS para a loja) e ganha uma tabela de tokens no seu banco.
- **Orders**: contrato do registro do pedido, validação e mensagem; o Orders deixa de tocar em dados de cartão.
- **Front-end**: nova chamada antes do pedido e uma URL a mais embutida no build.
- **Mensageria**: o contrato da solicitação muda; mensagens antigas nas filas precisam ser descartadas antes da troca.
- **Dependências**: `allow-payment-retry` reusa este contrato (um token novo por tentativa). `add-payment-idempotency` e `use-catalog-price-in-orders` alteram os mesmos handlers; quem for aplicado depois rebaseia.

## Fora do escopo

- Gateway e cofre reais (o serviço continua simulado, com a regra do final `0000`).
- Guardar o cartão para compras futuras (token reutilizável).
- Autenticação no serviço de tokenização (gateways reais usam chave pública; o token sozinho não cobra nada).
- Bandeira do cartão e 3-D Secure.
