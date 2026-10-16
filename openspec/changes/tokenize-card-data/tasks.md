# Tasks

## 1. Tokenização no Payments

- [ ] 1.1 Criar a entidade `CardToken` (Id `tok_...`, titular, `Last4`, validade, `CreatedAt`, `ExpiresAt` +24 h, `UsedAt`) no `Payments.Domain`, o mapeamento no `PaymentContext` (schema `payment`) e `ICardTokenRepository`; verificar com `dotnet build Scorponok.slnx`
- [ ] 1.2 Mapear `POST /card-tokens` no `Payments.Web.Api` (validação igual à do checkout → `400 { errors }`; `201 { token, last4, expiresAt }`), CORS com `Cors:AllowedOrigins` e a requisição no `Payments.Web.Api.http`; o CVV não é gravado nem logado
- [ ] 1.3 Testes no `Payments.Tests`: cartão válido gera token sem número completo nem CVV; CVV inválido → `400`; verificar com `dotnet test src/backend/Payments/Payments.Tests`

## 2. Contrato da mensagem e cobrança pelo token

- [ ] 2.1 Trocar `CardData` por `string CardToken` no `PaymentRequested` (`Shared.Code`) e ajustar `ProcessPaymentCommand`, `PaymentRequestedConsumer` e `IPaymentGateway`/`PaymentRequest` (titular e `Last4` do token, sem número nem CVV); verificar com `dotnet build Scorponok.slnx`
- [ ] 2.2 No `ProcessPaymentHandler`: resolver o token; desconhecido, vencido ou usado por outro pedido → `Payment` recusado com `Token do cartão inválido ou vencido.` e `PaymentRejected`; válido → cobrar e marcar `UsedAt` na mesma gravação do `Payment`; reentrega do mesmo pedido continua republicando o resultado
- [ ] 2.3 Atualizar `ProcessPaymentHandlerTests`, os testes do gateway e do consumidor: token válido aprovado, final `0000` recusado, token vencido, token usado por outro pedido, reentrega idempotente; verificar com `dotnet test src/backend/Payments/Payments.Tests`

## 3. Orders sem dados de cartão

- [ ] 3.1 Trocar `CreditCardPaymentCommand Card` por `PaymentMethod(CardToken, Installments)` (JSON `payment`) no `CreateCommand`; pagamento presente sem token → erro `Informe o cartão.` (`400`); `OrderHandler` publica o `PaymentRequested` com o token
- [ ] 3.2 Atualizar `OrderHandlerTests` e `CreateOrderEndpointTests`: solicitação leva só o token e as parcelas, corpo com número/CVV não chega à mensagem, pagamento sem token → `400`, pedido sem pagamento continua sem solicitação; verificar com `dotnet test src/backend/Store/Orders.Tests`

## 4. Front-end

- [ ] 4.1 Criar `src/lib/payments.ts` (`tokenizeCard`, `VITE_PAYMENTS_API_URL` com padrão `http://localhost:5220`) e o build arg no `Dockerfile` e no `docker-compose.frontend.yml`
- [ ] 4.2 No `CheckoutPage`: tokenizar antes do `createOrder`, enviar `payment: { cardToken, installments }` (sem `card`), mapear `400` da tokenização para os campos do cartão e falha de rede para a mensagem de cartão indisponível, mantendo carrinho e formulário; ajustar `CreateOrderRequest`
- [ ] 4.3 Rodar `npm run build && npm run lint` em `src/frontend`

## 5. Troca do contrato e verificação ponta a ponta

- [ ] 5.1 Com o compose: parar `orders-api` e `payment-api`, esvaziar `payment-requested` e `payment-requested_error`, recriar os volumes `orders_data` e `payment_data` e subir na ordem `payment-api`, `orders-api`, `frontend`
- [ ] 5.2 Checkout no navegador com cartão final `1111` termina "Aprovado" e com final `0000` termina "Recusado"; com `docker compose stop payment-api`, a confirmação mostra a mensagem de cartão indisponível e nenhum pedido é criado
- [ ] 5.3 Conferir que nenhum dado de cartão sobra: `OutboxMessage` do `orders-db`, mensagens das filas no painel do RabbitMQ (`payment-requested`) e as tabelas `Payments` e `CardTokens` não têm número completo nem CVV

## 6. Documentação

- [ ] 6.1 Atualizar o `CLAUDE.md` (tokenização no Payments, `POST /card-tokens`, CORS no Payments, contrato do `POST /createOrder` com `payment`, fim da frase sobre cartão e CVV no `orders-db` e nas filas, passos de troca do contrato), o `CLAUDE.md` do front-end e o `usecases/UC03`/`UC04` (remover a limitação do cartão no serviço de pedidos)
- [ ] 6.2 Rodar `dotnet test Scorponok.slnx`, todos sem falhas

## Workflow follow-up

- `allow-payment-retry` depende desta change (um token novo por tentativa).
- Aplicar depois de `add-payment-idempotency` e `use-catalog-price-in-orders`, ou rebasear os handlers em comum.
- Antes de arquivar, reescrever o MODIFIED "Registro do pagamento" (`payment-processing`) sobre o texto já arquivado por `add-payment-idempotency`, que altera o mesmo requisito; senão, o arquivamento desfaz aquela mudança.
- Arquivar esta change (`/opsx:archive`) depois da revisão.
