# Design

## Context

A conta do cliente vive só no Keycloak; o Orders lê `sub` e `email` do token (política `VerifiedCustomer`) e guarda o endereço como `street`, `city`, `state`, `country`, `zipCode`. Os tokens do `scorponok-web` levam só o audience `orders-api`.

## Goals / Non-Goals

**Goals:** contexto **Customers** no molde do Orders; login do Keycloak reaproveitado; checkout e pedido funcionando sem o Customers no ar.

**Non-Goals:** atualizar cadastro existente pela compra; entrega fora do Brasil; perfil no Keycloak.

## Decisions

### 1. Bounded context `Customers`
`Customers.Domain` ← `.CommandHandlers` ← `.Infrastructure` ← `.Web.Api`, mais `Customers.Tests`, em `src/backend/Customers/`. Compose: `customers-api` (5230) e `customers-db` (5437, volume `customers_data`).
- *Alternativa:* cadastro dentro do Orders — mistura cadastro com pedido.

### 2. Agregado `Customer` com `Id` = `sub`
`Email`, `FullName`, `Phone`, `Cpf` e o value object `Address` (`Street`, `Number`, `Complement`, `District`, `City`, `State`, `ZipCode`), owned type na tabela `Customers`. Telefone, CPF e CEP só com dígitos.

### 3. Validação
FluentValidation com as regras da spec, após remover a pontuação; erros nas chaves do JSON (`cpf`, `address.number`). CPF, telefone, UF e CEP em `Shared.Code/Validation`, usados também pelo Orders.

### 4. API `GET`/`PUT /customers/me`
`VerifiedCustomer`, cliente só do token, via `IMemoryBus.RequestAsync`. `GET` sem cadastro devolve só o e-mail; `PUT` é upsert (`200` ou `400 { errors }`). Autenticação copiada do Orders com `Audience` = `customers-api` (mapper `customers-api-audience`); health `live`, `ready` (banco) e `bus`; sem log do corpo.
- *Alternativa:* autenticação no `Shared.Code` — fica para depois, para não mexer no Orders.

### 5. Pedido com endereço estruturado e comprador
`OrderAddress` ganha `Number`, `Complement` e `District`, com UF e CEP validados e `Country` = `Brasil`. `POST /createOrder` ganha `customer { fullName, phone, cpf }`, validado e não persistido. Exige recriar `orders_data`.
- *Alternativa:* endereço estruturado só na mensagem — dois endereços para o mesmo pedido.

### 6. Mensagem `OrderPlaced` pelo outbox
Publicada pelo `OrderHandler` junto com o `PaymentRequested`, para todo pedido, com cliente, e-mail, comprador e `ShippingAddress`. No Customers, a fila `order-placed` (com `UseTransientRetry`) leva ao `RecordPurchaseCommand`, que grava o endereço em `DeliveryAddresses` (índice único em `OrderNumber`) e cria o `Customer` só se não existir. Entregas repetidas ou simultâneas caem no índice ou na chave `sub`, e a retentativa relê.
- *Alternativa:* Customers consultar o Orders — acoplamento síncrono.

### 7. Front-end
`src/lib/customers.ts` (`VITE_CUSTOMERS_API_URL`); `ProfilePage` em `/account/profile`; menu da conta no `Header`. Checkout com nome, telefone e CPF e `ShippingAddressForm` no formato do cadastro, pré-preenchidos só nos campos vazios; falha na busca é ignorada.

## Risks / Trade-offs

- [Recriar `orders_data` apaga os pedidos] → ambiente de estudo.
- [CPF passa pelo outbox e pela fila] → rede interna, linha apagada após a entrega, sem log.
- [Mensagem anterior à fila `order-placed` é descartada] → subir o `customers-api` primeiro.
- [Mapper só entra reimportando o realm] → ajuste pelo console (*Clients* → `scorponok-web` → *Client scopes*).

## Migration Plan

1. Subir `customers-api`.
2. Adicionar o audience `customers-api` no realm.
3. Recriar `orders_data` e reconstruir `orders-api` e `frontend` juntos.

Rollback: remover o Customers, o `customer` e o `OrderPlaced` do pedido, voltar o endereço e recriar `orders_data`.
