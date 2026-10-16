# Design

## Context

A motivação está em `proposal.md`. O estado atual que molda a abordagem:

- O `PaymentForm`/`CheckoutPage` monta `card` (`cardHolderName`, `cardNumber`, `expirationMonth`, `expirationYear`, `cvv`, `amount`, `installments`) e o `createOrder` (`src/lib/orders.ts`) o envia no `POST /createOrder`.
- No Orders, o `CreateCommand` tem `CreditCardPaymentCommand Card`; o `OrderHandler` publica `PaymentRequested(..., CardData Card)` pelo bus outbox, então o cartão fica na `OutboxMessage` do `orders-db` até a entrega. Pedido sem `Card` é aceito e não gera solicitação.
- `CardData` e `PaymentRequested` ficam em `Shared.Code/IntegrationEvents` e são usados pelos dois lados.
- No Payments, o `ProcessPaymentHandler` monta `PaymentRequest` com número e CVV para o `IPaymentGateway`; o `PaymentGateway` simulado decide pelo final `0000`; `Payment` já guarda só `CardHolderName` e `CardLast4`. O `Payments.Web.Api` não tem endpoints de negócio, nem CORS, nem autenticação.
- O retry do MassTransit manda mensagens que falham para `payment-requested_error`, onde ficam sem consumidor.

## Goals / Non-Goals

**Goals:**
- Nenhum CVV gravado em lugar nenhum (banco, outbox, fila, log); nenhum número completo fora da requisição de tokenização.
- O Orders não conhece dados de cartão.
- Contrato pronto para uma nova tentativa com outro cartão (`allow-payment-retry`).

**Non-Goals:**
- Criptografia de um cofre real; o token simulado só guarda o que a decisão simulada precisa.
- Autenticar a tokenização.

## Decisions

### 1. Tokenização no Payments.Web.Api
`POST /card-tokens` com `{ cardHolderName, cardNumber, expirationMonth, expirationYear, cvv }`:
- valida como o checkout (13 a 19 dígitos, CVV de 3 ou 4, validade não vencida) → `400 { errors }`;
- grava `CardToken` (schema `payment`): `Id` (`tok_` + GUID sem hífens), `CardHolderName`, `Last4`, `ExpirationMonth`, `ExpirationYear`, `CreatedAt`, `ExpiresAt` (+24 h), `UsedAt`;
- responde `201 { token, last4, expiresAt }`.

O CVV só é conferido em memória; o número completo nunca é gravado. Como o simulador decide pelo final do cartão, `Last4` basta para a regra `0000`. CORS com `Cors:AllowedOrigins` (`http://localhost:3000`), como no Catalog e no Orders. Sem autenticação: o token sozinho não cobra nada e só vale num pedido do cliente autenticado.

- *Alternativa:* tokenizar no Orders: o cartão continuaria passando pelo Orders, que é o que se quer evitar.
- *Alternativa:* novo serviço "gateway" no compose: mais realista, mas uma API a mais para um simulador; o Payments já é o dono do gateway.

### 2. Validade de 24 h e uso único
O token não é dado sensível (titular, final e validade), então a validade pode cobrir o atraso do outbox com o broker fora. 24 h cobre quedas longas do broker sem guardar nada de PCI. O uso é marcado (`UsedAt`) na mesma transação em que o `Payment` é gravado.

- *Alternativa:* 15 minutos, como gateways reais: com o broker fora, pedidos legítimos falhariam por token vencido.

### 3. Token inválido vira recusa, não erro
Token desconhecido, vencido ou usado em outro pedido: o handler grava o `Payment` recusado com o motivo `Token do cartão inválido ou vencido.` e publica `PaymentRejected`. O pedido termina `Failed` e o cliente recebe o e-mail; nada vai para a fila de erro. Reentrega do mesmo pedido continua idempotente pelo `Payment` já gravado (o token usado pelo próprio pedido não conta como "usado em outro").

### 4. Contratos
- `CardData` sai. `PaymentRequested(int OrderId, string OrderNumber, Guid CustomerId, decimal Amount, int Installments, string CardToken)`.
- `CreateCommand.Card` vira `PaymentMethod(string CardToken, int Installments)` (campo JSON `payment`). Ausente → pedido sem pagamento, como hoje; presente sem token → `400` (`Informe o cartão.`). Campos antigos de cartão no corpo são ignorados pelo binder e nunca chegam ao handler.
- `IPaymentGateway.SendProcessPaymentAsync(PaymentRequest)` passa a receber o `CardToken` resolvido (titular, `Last4`) em vez de número e CVV.
- `allow-payment-retry`: cada tentativa gera um token novo no navegador e usa o mesmo `PaymentRequested`; nada no contrato prende um pedido a um único token.

### 5. Front-end
Antes do `createOrder`, `tokenizeCard(card)` (`src/lib/payments.ts`, `VITE_PAYMENTS_API_URL`, padrão `http://localhost:5220`) chama o `POST /card-tokens`. `400` vira erro dos campos do cartão; falha de rede ou outro status vira a mensagem de cartão indisponível, sem criar o pedido. O token não é guardado em `localStorage` nem reaproveitado entre envios.

### 6. Troca do contrato e limpeza
O contrato muda nos dois lados ao mesmo tempo, e não há migrations:
1. parar `orders-api` e `payment-api`;
2. esvaziar `payment-requested` e `payment-requested_error` no RabbitMQ (mensagens com `CardData`, inclusive CVV);
3. recriar os volumes `orders_data` (remove a `OutboxMessage` com cartão) e `payment_data` (cria a tabela de tokens);
4. subir `payment-api`, depois `orders-api` e o `frontend` reconstruído.

## Risks / Trade-offs

- [Endpoint de tokenização aberto] → só gera tokens; cobrança exige pedido autenticado. Rate limit fica para depois.
- [Recriar volumes apaga pedidos e pagamentos de teste] → aceitável em ambiente de estudo; documentado no `CLAUDE.md`.
- [Navegador fala com mais uma API] → mais uma origem no CORS e mais uma URL no build do front-end.
- [Token pode vencer com o broker fora mais de 24 h] → o pedido falha com motivo claro e o cliente refaz a compra (ou tenta de novo com `allow-payment-retry`).

## Migration Plan

Passos da decisão 6. Rollback: reverter o commit e recriar os mesmos volumes; o contrato antigo volta inteiro.
