# Tasks

## 1. Orders: contrato e validação

- [x] 1.1 Criar o projeto de testes `src/backend/Store/Orders.Tests` (NUnit + FluentAssertions + Moq, versões e target pelo `Directory.Build.props`) e adicioná-lo à `Scorponok.sln`; verificar com `dotnet test src/backend/Store/Orders.Tests` rodando zero testes sem erro
- [x] 1.2 Trocar `ProductId` de `Guid` para `int` em `OrderItem` (incluindo `RemoveItem`), `OrderItemConfigurations`, `OrderBuilder.CreateItem` e `OrderItemMessageResponse`; verificar com `dotnet build Scorponok.sln`
- [x] 1.3 No `OrderValidation`, remover a regra de `PaymentId` e exigir `Items` não vazio; verificar com testes em `Orders.Tests` cobrindo pedido válido sem `PaymentId`, pedido sem itens, item com quantidade 0 e endereço sem cidade
- [x] 1.4 Adicionar `Message<TResponse>` em `Shared.Code` e `RequestAsync<TResponse>` ao `IMemoryBus`/`MemoryBus`; verificar com `dotnet build Scorponok.sln` e com um teste que envia um comando de resposta por um `MemoryBus` com `IMediator` mockado
- [x] 1.5 Criar `CreateOrderResult` e fazer o `CreateCommand` herdar `Message<CreateOrderResult>`. O `OrderHandler` deve usar o `UserId` como `CustomerId`, chamar `IsValid()`, salvar só pedidos válidos e devolver número, status, total e erros. Verificar com testes do handler (repositório mockado): válido → `Save` chamado, `CustomerId` = `UserId`, total correto; inválido → `Save` não chamado e erros preenchidos

## 2. Orders: endpoint e CORS

- [x] 2.1 Fazer o `CreateOrder` responder `201 Created` com `{ orderNumber, status, total }` ou `400` com `{ errors }`, e atualizar o `Orders.Web.Api.http` com um `POST /createOrder` válido e um inválido; verificar subindo a API (`docker compose -f docker-compose.database.yml up -d` + `dotnet run --project src/backend/Store/Orders.Web.Api`) e executando as duas requisições do `.http`
- [x] 2.2 Configurar CORS no Orders igual ao Catalog (`Cors:AllowedOrigins` = `http://localhost:3000` no `appsettings.json`, `AddCors` e `UseCors()`); verificar com `curl -i -X OPTIONS http://localhost:5224/createOrder -H "Origin: http://localhost:3000" -H "Access-Control-Request-Method: POST"` devolvendo `Access-Control-Allow-Origin`, e sem o cabeçalho para `Origin: http://exemplo.com`
- [x] 2.3 Recriar o volume do `orders-db` e confirmar que o `POST` válido persiste o pedido com `ProductId` inteiro e status `Pending` (consulta `psql` nas tabelas de pedido e itens)
- [x] 2.4 Atualizar o `CLAUDE.md` da raiz (contrato do `POST /createOrder`, CORS no Orders, necessidade de recriar o banco do Orders) e verificar que os comandos citados rodam como descritos

## 3. Front-end: base

- [x] 3.1 Criar `src/lib/orders.ts` (`createOrder`, tipos de request e response, `VITE_ORDERS_API_URL` com padrão `http://localhost:5224`) e `src/lib/customer.ts` (`getCustomerId` com `crypto.randomUUID()` guardado no `localStorage`); verificar com `npm run build` e `npm run lint`
- [x] 3.2 Adicionar o build arg `VITE_ORDERS_API_URL` ao `src/frontend/Dockerfile` e ao `docker-compose.frontend.yml`; verificar com `docker compose -f docker-compose.frontend.yml build`
- [x] 3.3 Remover frete, imposto e o aviso de "free shipping" do `OrderSummary`, deixando total = subtotal; verificar no navegador que o carrinho mostra só subtotal e total iguais

## 4. Front-end: página de checkout

- [x] 4.1 Criar a rota `/checkout` no `App.tsx` e a `CheckoutPage`, com redirecionamento para `/cart` quando o carrinho está vazio; verificar no navegador que "Proceed to Checkout" abre a página e que `/checkout` com o carrinho vazio volta para `/cart`
- [x] 4.2 Criar `CheckoutItems` (itens só leitura com subtotal por linha e link "Edit cart"); verificar no navegador com 2 produtos de quantidades diferentes
- [x] 4.3 Criar `ShippingAddressForm` e `PaymentForm` com a validação por campo (obrigatórios, cartão de 13 a 19 dígitos, CVV de 3 ou 4 dígitos, validade não vencida, parcelas padrão 1); verificar no navegador que cada campo inválido bloqueia o envio e mostra a mensagem
- [x] 4.4 Criar `CheckoutSummary` (total = soma dos itens, botão "Place order" desabilitado durante o envio) e ligar o envio ao `createOrder` com `getCustomerId()`; verificar na aba Network que o `POST /createOrder` sai com itens, endereço, cartão e `userId`
- [x] 4.5 Tratar o resultado: sucesso mostra `OrderConfirmation` com número e status e chama `clearCart()`; `400` mostra as mensagens; falha de rede mostra um aviso genérico, mantendo carrinho e formulário. Verificar no navegador os três casos (API no ar, payload inválido forçado e API parada)
- [x] 4.6 Atualizar o `src/frontend/CLAUDE.md` (rota `/checkout`, `orders.ts`, `customer.ts`, `VITE_ORDERS_API_URL`, resumo sem frete e imposto); verificar que a descrição bate com o código

## 5. Integração

- [x] 5.1 Subir tudo com `docker compose up -d --build` e fazer o fluxo completo pela loja em `http://localhost:3000`: adicionar produtos, finalizar e ver o número do pedido. Confirmar o pedido no `orders-db` com o mesmo `CustomerId` em dois pedidos seguidos
- [x] 5.2 Rodar `dotnet test Scorponok.sln` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas

## Workflow follow-up

- Arquivar a change (`/opsx:archive`) depois da revisão, para criar as specs `checkout` e `order-placement` em `openspec/specs/`.
