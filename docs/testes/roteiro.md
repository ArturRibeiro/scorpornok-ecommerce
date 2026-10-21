# Roteiro de testes

Testes manuais do fluxo do pedido e do pagamento, de ponta a ponta, com o ambiente do compose.

## Preparação

- Suba tudo: `docker compose up -d --build` (loja em http://localhost:3000, painel do RabbitMQ em http://localhost:15672, guest/guest).
- Os e-mails saem por um SMTP de verdade (ex.: Gmail): configure `SMTP_HOST`, `SMTP_PORT`, `SMTP_FROM`, `SMTP_USERNAME`, `SMTP_PASSWORD` e `SMTP_USE_STARTTLS=true` no `.env` da raiz. Os e-mails do pedido chegam na caixa de `arturrj@gmail.com` (confira também o spam).
- Confira o status em "Meus pedidos" na loja (menu da conta), pelo e-mail recebido, pelo `myOrder` no gateway (ou o `GET /orders/{orderNumber}` do REST) ou na tabela do `orders-db`.
- Para seguir um pedido por dentro (gateway, Orders, Payments, Customers e RabbitMQ), suba com o painel de telemetria: `OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889 docker compose --profile telemetry-local up -d --build` (painel em http://localhost:18888). Em *Traces*, filtre por `order.number` com o número do pedido (cenário 34).
- Anote o número de cada pedido (tela de confirmação da loja ou `orderNumber` da resposta do `POST /createOrder`): os cenários seguintes usam os pedidos pelos apelidos abaixo (P1, P2, ...).
- As chamadas diretas à API estão em `src/backend/Store/Orders.Web.Api/PaymentRetry.http` (nova tentativa), `src/backend/Store/Orders.Web.Api/Orders.Web.Api.http` (pedido e histórico), `src/backend/Gateway/Gateway.Web.Api/Gateway.Web.Api.http` (catálogo, cadastro, pedidos e tokenização pelo gateway GraphQL) e `src/backend/Customers/Customers.Web.Api/Customers.Web.Api.http` (cadastro). Os cenários marcados com **API** não dão para fazer pela loja.

## Dados comuns

### Contas

Todos os cenários usam a conta A logada na loja. A conta B entra nos cenários 10, 17, 22, 23 e 27; cadastre-a pela loja e confirme pelo link no e-mail.

| Conta | E-mail | Senha |
|---|---|---|
| A | `arturrj@gmail.com` | a da conta |
| B | um segundo e-mail seu (precisa receber o link de confirmação) | a definida no cadastro |

Access token (para o `.http`, vale 5 min): na loja, `localStorage`, chave `oidc.user:http://localhost:8080/realms/scorponok:scorponok-web`, campo `access_token`.

### Comprador e endereço

O checkout pede nome, telefone e CPF e o endereço no formato do cadastro (sempre no Brasil, complemento opcional). Com o cadastro salvo, os campos já vêm preenchidos; na primeira compra, preencha com os dados abaixo (eles criam o cadastro).

| Campo | Valor |
|---|---|
| Nome completo | Fulano de Tal |
| Telefone | (21) 99999-8888 |
| CPF | 529.982.247-25 |
| Endereço | Rua das Flores, 100, Centro, Rio de Janeiro, RJ, CEP 20040-020 |

### Regra dos cartões

O meio de pagamento simulado recusa cartão terminado em `0000` e aprova os demais. A validade precisa ser do mês atual em diante e o CVV ter 3 ou 4 dígitos. Cada cartão digitado vira um token novo, que vale uma cobrança só.

## Cenários

### 1. Pedido aprovado

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 12/2030 |
| CVV | 123 |

1. Logado como `arturrj@gmail.com`, finalize a compra com os dados acima.
2. **Esperado:** P1 `Confirmed`; a loja mostra a aprovação; e-mail de pagamento aprovado em `arturrj@gmail.com`.

### 2. Pedido recusado

| Campo | Valor |
|---|---|
| Cartão | `4000 0000 0000 0000` |
| Titular | Fulano de Tal |
| Validade | 06/2029 |
| CVV | 321 |

1. Finalize com os dados acima.
2. **Esperado:** P2 `Failed` (tentativa 1 de 3); a loja mostra a recusa e oferece outro cartão; e-mail de pagamento recusado em `arturrj@gmail.com`, dizendo que restam 2 tentativas.

### 3. O pedido não consegue enviar o pagamento porque o RabbitMQ está fora

| Campo | Valor |
|---|---|
| Cartão | `5555 5555 5555 4444` |
| Titular | Fulano de Tal |
| Validade | 03/2031 |
| CVV | 456 |

1. `docker compose stop rabbitmq`.
2. Finalize com os dados acima.
3. **Esperado:** `201` e P3 `Pending`; a mensagem fica na tabela `OutboxMessage` do `orders-db`; `http://localhost:5224/health/bus` responde `503` e `http://localhost:5224/health/ready` continua `200`.
4. `docker compose start rabbitmq`.
5. **Esperado:** o outbox entrega a mensagem, a linha some da `OutboxMessage` e P3 passa a `Confirmed`; e-mail de pagamento aprovado em `arturrj@gmail.com`.

### 4. A API de pagamento está fora do ar

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 11/2028 |
| CVV | 789 |

1. `docker compose stop payment-api`.
2. Finalize com os dados acima.
3. **Esperado:** P4 `Pending`; a mensagem espera na fila `payment-requested` (painel do RabbitMQ).
4. `docker compose start payment-api` antes do prazo da consulta (15 min por padrão).
5. **Esperado:** o Payments processa a mensagem e P4 passa a `Confirmed`; e-mail de pagamento aprovado em `arturrj@gmail.com`.

**Variante: fora além do prazo** (gera o pedido usado no cenário 16)

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 08/2032 |
| CVV | 234 |

1. Para não esperar 15 min, crie um `.env` na raiz com `PAYMENT_CHECK_TIMEOUT_MINUTES=1` e `PAYMENT_CHECK_INTERVAL_SECONDS=15` e aplique com `docker compose up -d orders-api`.
2. `docker compose stop payment-api` e finalize com os dados acima.
3. Espere passar o prazo (cerca de 1 min) e `docker compose start payment-api`.
4. **Esperado:** P4b `NotProcessed`; nada é cobrado (o pagamento fica `Voided` no `payment-db`); e-mail em `arturrj@gmail.com` avisando que nada foi cobrado e que é preciso refazer a compra.
5. Remova as duas variáveis do `.env` e rode `docker compose up -d orders-api` de novo.

### 5. A API de pagamento não consegue enviar o resultado para o pedido

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 05/2030 |
| CVV | 567 |

1. Finalize com os dados acima e, logo em seguida, `docker compose stop orders-api`.
2. **Esperado:** o pagamento fica `Approved` no `payment-db` e o `PaymentApproved` espera na fila `payment-approved`.
3. `docker compose start orders-api`.
4. **Esperado:** P5 passa a `Confirmed`; um único e-mail de pagamento aprovado em `arturrj@gmail.com`.
5. Variante: com o resultado parado em `payment-approved_error`, o `PendingOrderWatcher` consulta o Payments depois do prazo e o resultado é republicado; o pedido termina `Confirmed`. O `/health/ops` fica `503` enquanto houver mensagem na fila `_error`.

### 6. Recusa e aprovação na 2ª tentativa

| Tentativa | Cartão | Titular | Validade | CVV | Resultado |
|---|---|---|---|---|---|
| 1 (pedido) | `4000 0000 0000 0000` | Fulano de Tal | 12/2030 | 123 | `Failed` |
| 2 | `5555 5555 5555 4444` | Fulano de Tal | 09/2031 | 654 | `Confirmed` |

1. Finalize com a tentativa 1; depois da recusa, tente de novo com a tentativa 2.
2. **Esperado:** `202` com `attempt: 2`; P6 passa a `Confirmed` com o mesmo total do pedido; em `arturrj@gmail.com`, o e-mail da recusa (restam 2 tentativas) e o da aprovação.

### 7. Tentativas esgotadas

| Tentativa | Cartão | Titular | Validade | CVV | Resultado | E-mail |
|---|---|---|---|---|---|---|
| 1 (pedido) | `4000 0000 0000 0000` | Fulano de Tal | 12/2030 | 123 | `Failed` | restam 2 tentativas |
| 2 | `5555 0000 0000 0000` | Fulano de Tal | 07/2029 | 852 | `Failed` | resta 1 tentativa |
| 3 | `4000 0000 0000 0000` | Fulano de Tal | 01/2033 | 963 | `Failed` | as tentativas acabaram |

1. Finalize com a tentativa 1 e tente de novo com as tentativas 2 e 3.
2. **Esperado:** `Failed` em cada tentativa, com os e-mails acima em `arturrj@gmail.com`; depois da 3ª, a loja informa que é preciso fazer um novo pedido.

### 8. Limite de tentativas (API)

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 12/2030 |
| CVV | 123 |

1. Gere o token do cartão no `POST /card-tokens` e chame `POST /orders/{P7}/payment` com ele.
2. **Esperado:** `409`; nada é cobrado e P7 continua `Failed`.

### 9. Pedido não recusado (API)

| Campo | Valor |
|---|---|
| Cartão | `5555 5555 5555 4444` |
| Titular | Fulano de Tal |
| Validade | 10/2029 |
| CVV | 456 |

1. Gere o token do cartão e chame `POST /orders/{P6}/payment` com ele.
2. **Esperado:** `409`; P6 continua `Confirmed`.

### 10. Pedido de outro cliente (API)

Crie antes, pela loja, um pedido recusado de A, usado também nos cenários 12 e 13:

| Campo | Valor |
|---|---|
| Cartão | `4000 0000 0000 0000` (guarde o token: `tok10`) |
| Titular | Fulano de Tal |
| Validade | 04/2030 |
| CVV | 147 |

Tentativa do cliente B:

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Ciclano de Tal |
| Validade | 02/2031 |
| CVV | 258 |

1. Gere o token do cartão de B e, com o access token de B, chame `POST /orders/{P10}/payment`.
2. **Esperado:** `404` (não revela que o pedido existe); P10 continua `Failed` na tentativa 1.

### 11. Pedido inexistente (API)

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 12/2030 |
| CVV | 123 |

1. Gere o token do cartão e chame `POST /orders/A000000000000/payment` com ele.
2. **Esperado:** `404`.

### 12. Sem cartão (API)

| Campo | Valor |
|---|---|
| Cartão | nenhum: corpo `{ "installments": 1 }`, sem `cardToken` |

1. Chame `POST /orders/{P10}/payment` com o corpo acima.
2. **Esperado:** `400` (`Informe o cartão.`); P10 continua na tentativa 1.

### 13. Token já usado (API)

| Campo | Valor |
|---|---|
| Cartão | `tok10`: o token do `4000 0000 0000 0000`, validade 04/2030, CVV 147, já usado na tentativa 1 |

1. Chame `POST /orders/{P10}/payment` com `tok10`.
2. **Esperado:** `202` com `attempt: 2`; depois P10 fica `Failed` com o motivo `Token do cartão inválido ou vencido.`, sem passar pelo gateway; e-mail de pagamento recusado em `arturrj@gmail.com`, dizendo que resta 1 tentativa.

### 14. Carrinho na recusa

| Campo | Valor |
|---|---|
| Cartão | `4000 0000 0000 0000` |
| Titular | Fulano de Tal |
| Validade | 10/2030 |
| CVV | 369 |

1. Finalize com os dados acima.
2. **Esperado:** depois da recusa, o carrinho continua com os mesmos itens (para tentar outro cartão ou fazer um novo pedido); e-mail de pagamento recusado em `arturrj@gmail.com`.

### 15. Carrinho no sucesso

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 07/2031 |
| CVV | 741 |

1. Finalize com os dados acima.
2. **Esperado:** o carrinho é esvaziado (também com o pedido ainda pendente, para não comprar duas vezes); e-mail de pagamento aprovado em `arturrj@gmail.com`.

### 16. Não processado não tem nova tentativa (API)

| Campo | Valor |
|---|---|
| Cartão | `4111 1111 1111 1111` |
| Titular | Fulano de Tal |
| Validade | 12/2030 |
| CVV | 123 |

1. Gere o token do cartão e chame `POST /orders/{P4b}/payment` com ele.
2. **Esperado:** `409`; P4b continua `NotProcessed`, e a loja orienta a refazer a compra.

### 17. Cadastro vazio e primeira compra

1. Com a conta B (sem compras e sem cadastro), abra "Meu cadastro" no menu da conta (o e-mail no cabeçalho).
2. **Esperado:** o e-mail da conta só para leitura e todos os campos em branco, sem erro.
3. Finalize uma compra com cartão aprovado, informando o comprador e o endereço de "Dados comuns".
4. **Esperado:** depois do pedido, "Meu cadastro" mostra o nome, o telefone, o CPF e o endereço informados no checkout.

### 18. Validação do cadastro

1. Em "Meu cadastro", informe o CPF `111.111.111-11` e salve.
2. **Esperado:** nada é salvo e o CPF é indicado como inválido.
3. Apague o número, o bairro e a cidade do endereço e salve.
4. **Esperado:** nada é salvo e os três campos são indicados como obrigatórios.
5. Corrija tudo (complemento vazio) e salve.
6. **Esperado:** "Cadastro salvo."; ao recarregar a página, os mesmos valores, com a pontuação.

### 19. Checkout pré-preenchido e entrega em outro endereço

1. Com o cadastro salvo no Rio de Janeiro, coloque um item no carrinho e abra o checkout.
2. **Esperado:** comprador e endereço já preenchidos com o cadastro.
3. Troque o endereço para Av. Paulista, 1000, Bela Vista, São Paulo, SP, CEP 01310-100 e o telefone para (11) 98888-7777, e finalize com cartão aprovado.
4. **Esperado:** o pedido é registrado com o endereço de São Paulo (tabela `OrderAddress` do `orders-db`, país `Brasil`); "Meu cadastro" continua com o Rio de Janeiro e o telefone anterior; o endereço de São Paulo entra no histórico (tabela `customer."DeliveryAddresses"` do `customers-db`, com o número do pedido).

### 20. Customers fora do ar

1. `docker compose stop customers-api` e abra o checkout.
2. **Esperado:** comprador e endereço em branco, e o checkout funciona normalmente; "Meu cadastro" mostra que não foi possível carregar.
3. Finalize uma compra com cartão aprovado.
4. **Esperado:** o pedido é registrado e aprovado; o `OrderPlaced` espera na fila `order-placed` (painel do RabbitMQ).
5. `docker compose start customers-api`.
6. **Esperado:** o endereço do pedido entra no histórico (e o cadastro é criado, se for a primeira compra da conta).

### 21. Falha ao salvar o cadastro

1. Abra "Meu cadastro", `docker compose stop customers-api`, altere um campo e salve.
2. **Esperado:** a mensagem de que não foi possível salvar, com os campos como foram digitados.
3. `docker compose start customers-api`.

### 22. Cadastro só do dono (API)

1. Com o token da conta A, `GET /customers/me` no `Customers.Web.Api.http`; repita com o token da conta B.
2. **Esperado:** cada conta vê só o próprio cadastro; não há como informar outra conta na rota ou no corpo.

### 23. Meus pedidos

Use a conta A depois dos cenários anteriores (vários pedidos: aprovados, recusados e não processados) e a conta B do cenário 17.

1. Com a conta A, abra "Meus pedidos" no menu da conta.
2. **Esperado:** os pedidos de A, do mais recente para o mais antigo, cada um com número, data, situação ("Pago", "Pagamento recusado", "Pagamento não processado"), total, quantidade de itens e foto; nenhum pedido de B.
3. Se houver mais de 10 pedidos, acione "Carregar mais" até o botão sumir.
4. **Esperado:** todos os pedidos aparecem uma vez só, e o botão some no último bloco.
5. Abra P1.
6. **Esperado:** os itens com preço unitário, quantidade e subtotal (sem desconto, que hoje é sempre zero), o total, o endereço de entrega do checkout e a situação "Pago".
7. Com uma conta nova, sem compras, abra "Meus pedidos".
8. **Esperado:** "Você ainda não fez nenhuma compra", com o link para a loja.

### 24. Acompanhar o pagamento pelo pedido

1. `docker compose stop payment-api` e finalize uma compra com cartão aprovado (P24).
2. Na confirmação, acione "Ver meus pedidos" e abra P24.
3. **Esperado:** "Aguardando pagamento", com o aviso de que a página será atualizada.
4. `docker compose start payment-api`, sem recarregar a página.
5. **Esperado:** em alguns segundos, P24 passa a "Pago" sozinho.

### 25. Pagar de novo pelo pedido

1. Finalize uma compra com o cartão recusado do cenário 2 (P25) e saia da tela da confirmação.
2. Em "Meus pedidos", abra P25.
3. **Esperado:** "Pagamento recusado", restam 2 tentativas, e o formulário "Tentar com outro cartão".
4. Informe o cartão aprovado do cenário 1 e confirme.
5. **Esperado:** P25 volta a "Aguardando pagamento" e passa a "Pago" sem recarregar; e-mail de pagamento aprovado.
6. Abra um pedido com as 3 tentativas recusadas (cenário 7).
7. **Esperado:** "Pagamento recusado", sem o formulário, com a orientação de refazer a compra.

### 26. Pedido como foi comprado

1. Com P1 já registrado, altere o endereço em "Meu cadastro" e salve.
2. Abra P1 em "Meus pedidos".
3. **Esperado:** o endereço de entrega do pedido continua o do checkout, não o novo do cadastro.

### 27. Pedido de outro cliente

1. Com a conta B, abra `http://localhost:3000/account/orders/{P1}` (pedido de A).
2. **Esperado:** "Pedido não encontrado", igual a abrir um número que não existe (`/account/orders/A000000000000`).
3. **API:** com o token de B, `GET /orders/{P1}` no `Orders.Web.Api.http`.
4. **Esperado:** `404` com `Pedido não encontrado.`; sem token, `401`.

### 28. Catálogo pelo gateway GraphQL

1. Abra a home e o DevTools (aba Network); navegue até a página 2 e abra um produto.
2. **Esperado:** a mesma grade (18 por página), a mesma paginação e a mesma página de produto de antes; as chamadas são `POST` para `http://localhost:5200/graphql`, nenhuma para `localhost:5064`.
3. Abra `http://localhost:3000/product/999999`.
4. **Esperado:** "produto não encontrado".
5. `docker compose stop catalog-api` e recarregue a home.
6. **Esperado:** a mensagem de que não foi possível carregar os produtos, com "Tentar de novo"; `http://localhost:5200/health/ready` continua `200`.
7. `docker compose start catalog-api` e finalize uma compra.
8. **Esperado:** o pedido é registrado com o preço do catálogo, como antes (o Orders continua usando o REST do Catalog).

### 29. Limites do gateway (API)

1. No `Gateway.Web.Api.http`, envie uma consulta com mais de 200 campos (por exemplo, 40 vezes `productById(id: 1) { id name sku price description pictureUri }` com aliases).
2. **Esperado:** erro `MAX_FIELDS_EXCEEDED`, sem chamada ao Catalog (nada no log do `catalog-api`).
3. Envie a consulta de introspecção do `.http`.
4. **Esperado:** o schema em Development (o compose); fora dele, erro `HC0046`.

### 30. Cadastro pelo gateway GraphQL

1. Com a conta A logada, abra "Meu cadastro" e o checkout com o DevTools (aba Network).
2. **Esperado:** os mesmos dados de antes; as chamadas são `POST` para `http://localhost:5200/graphql` (`me` e `updateMyProfile`), nenhuma para `localhost:5230`.
3. Salve o CPF `111.111.111-11`.
4. **Esperado:** o CPF indicado como inválido e nada salvo, como no cenário 18.
5. No `localStorage`, troque o `access_token` da chave `oidc.user:...` por um valor qualquer e recarregue "Meu cadastro".
6. **Esperado:** a loja pede o login de novo (o gateway responde `401`).
7. **API:** decodifique o `access_token` (jwt.io) e confira que o `aud` tem `gateway-api`.

### 31. Pedidos pelo gateway GraphQL

1. Com a conta A, faça uma compra aprovada e outra recusada (cartões dos cenários 1 e 2), com o DevTools (aba Network) aberto.
2. **Esperado:** o registro é um `POST` para `http://localhost:5200/graphql` (`placeOrder`), nenhum para `localhost:5224/createOrder`; a confirmação acompanha o pagamento pelo hub (`localhost:5224/hubs/orders`), como antes.
3. Na recusa, pague com outro cartão.
4. **Esperado:** `retryOrderPayment` pelo gateway e o resultado na mesma tela.
5. `docker compose stop catalog-api` e confirme outra compra.
6. **Esperado:** a mensagem de que não foi possível enviar o pedido, com o carrinho mantido; nada registrado. `docker compose start catalog-api`.
7. Abra "Meus pedidos" e um pedido; com a conta B, abra o endereço de um pedido de A.
8. **Esperado:** a mesma lista e o mesmo detalhe de antes (`myOrders`/`myOrder` pelo gateway); para B, "Pedido não encontrado".

### 32. Tokenização pelo gateway GraphQL

1. Com o DevTools (aba Network) aberto, finalize uma compra com o cartão aprovado do cenário 1.
2. **Esperado:** a tokenização é um `POST` para `http://localhost:5200/graphql` (`tokenizeCard`), nenhum para `localhost:5220/card-tokens`; o `placeOrder` leva só o token.
3. Tente com o CVV `12`.
4. **Esperado:** o CVV indicado como inválido no formulário, nada enviado ao Orders.
5. `docker compose stop payment-api` e confirme de novo.
6. **Esperado:** a mensagem de que não foi possível validar o cartão; `docker compose logs gateway-api | grep 4111` não encontra nada. `docker compose start payment-api`.

### 33. Busca de produtos

Antes, com o código da busca, recrie o banco do Catalog (a coluna da marca e a extensão `unaccent` só entram num banco novo): `docker compose stop catalog-api catalog-db && docker compose rm -f catalog-db && docker volume rm scorpornok-ecommerce_catalog_data`, e depois `docker compose up -d --build catalog-api gateway-api frontend`.

1. Sem login, digite "n" no campo "Buscar produtos..." do cabeçalho e depois "nike".
2. **Esperado:** nada com "n"; com "nike", 5 sugestões com foto, nome e preço e "Ver todos os resultados (32)". Escolher uma sugestão abre o produto.
3. Digite "revolution tenis nike" (sem acento, fora de ordem) e tecle Enter.
4. **Esperado:** `/search?q=revolution tenis nike`, com os "Tênis Nike Revolution" primeiro e a quantidade encontrada.
5. Digite "a malha" no campo e depois "xyz".
6. **Esperado:** com "a malha" (só nas descrições), 5 sugestões e "Ver todos os resultados (20)", que abre os 20 tênis que citam "malha" na descrição; com "xyz", "Nenhum produto encontrado". Buscar "masculino" traz primeiro os que têm a palavra no nome e, nas últimas páginas, os que só a citam na descrição.
7. Busque "tenis", marque Nike e Adidas e informe o preço de 399,99 a 500.
8. **Esperado:** só Nike e Adidas nessa faixa, incluindo os de R$ 399,99; a URL guarda `brands`, `min` e `max`, e cada mudança volta à página 1. Abrir a mesma URL em outra aba mostra os mesmos resultados, os mesmos filtros e a mesma página.
9. Informe mínimo 500 e máximo 200 e acione "Aplicar".
10. **Esperado:** "A faixa é inválida", sem mudar os resultados nem a URL.
11. Marque só Skechers, até R$ 200.
12. **Esperado:** "Nenhum produto encontrado", com "Limpar filtros" (volta aos resultados do termo) e "Voltar à loja".
13. No celular (DevTools, largura de 390 px), abra a busca pela lupa e os filtros pelo botão "Filtros".
14. **Esperado:** as mesmas sugestões no painel, que fecha ao escolher; os filtros aparecem só ao tocar em "Filtros".
15. `docker compose stop catalog-api`, busque "nike" e digite no campo.
16. **Esperado:** a página de resultados mostra que não foi possível carregar os produtos, com "Tentar de novo"; as sugestões não aparecem, sem mensagem de erro. `docker compose start catalog-api`.

### 34. Telemetria no painel local

Antes, suba com o painel: `OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889 docker compose --profile telemetry-local up -d --build`.

1. Faça uma compra aprovada (cenário 1) e abra http://localhost:18888, em *Traces*.
2. **Esperado:** um trace `gateway-api: mutation PlaceOrder` com os cinco serviços: o `placeOrder` no `orders-api`, o `GET /GetProductsByIds` no `catalog-api`, o `outbox send`/`outbox process`, o `payment-requested` no `payment-api`, o `order-placed` no `customers-api` e o `payment-approved` de volta no `orders-api`. Em *Structured logs*, os logs do pedido levam ao mesmo trace.
3. Em *Traces*, filtre por `order.number` igual ao número do pedido; em *Structured logs*, procure o número.
4. **Esperado:** o trace da compra (spans do `orders-api`, do `payment-api` e do `customers-api` com o número), os da consulta do pedido e o do e-mail; nos logs, `Pedido … registrado.` e `Pedido …: pagamento Confirmed (tentativa 1).`, cada um com o link para o trace.
5. Em *Traces* e *Structured logs*, procure `4111` e o CPF do comprador.
6. **Esperado:** nada encontrado.
7. Deixe a loja parada por um minuto.
8. **Esperado:** nenhum trace novo (nem dos health checks, nem das rodadas sem trabalho); depois da compra, aparece um `orders-api: Envio dos e-mails de pagamento`, com um span por pedido.
9. `docker compose --profile telemetry-local stop aspire-dashboard` e faça outra compra.
10. **Esperado:** a compra segue normalmente. `docker compose --profile telemetry-local start aspire-dashboard`.

