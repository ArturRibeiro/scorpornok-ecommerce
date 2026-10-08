# Design

## Context

A motivação está em `proposal.md` (Why) e os requisitos em `specs/checkout` e `specs/order-placement`. O estado atual que molda a abordagem:

- **Front-end**: o carrinho vive no `localStorage` (`CartContext`, chave `cart`), com itens `{ id: number, name, price, image, quantity }`. O `id` é o mesmo `int` do Catalog. O único cliente HTTP é `src/lib/catalog.ts`, que lê a URL de `VITE_CATALOG_API_URL`. Não há biblioteca de formulários nem framework de testes. O `OrderSummary` calcula frete e imposto localmente.
- **Orders**:
  - `POST /createOrder` faz `IMemoryBus.SendAsync(command)` e responde 200 sem corpo. O `IMemoryBus` só tem `SendAsync(Message)`, sem retorno, e `Message` implementa `IRequest` (MediatR 13).
  - O `OrderHandler` usa `customerId: Guid.NewGuid()`, ignorando o `UserId`. Também nunca chama `Order.IsValid()` e salva direto.
  - O `OrderBuilder.AddPaymentMethod` executa a função e descarta o resultado, então `Order.PaymentMethod` fica `null`.
  - O `OrderValidation` exige `PaymentId` não vazio, então todo pedido novo seria inválido. Ele também não exige itens, porque `RuleForEach` passa com a lista vazia.
  - O `OrderAddressValidation` devolve códigos (`InvalidOrderAddressCityEmpty`) em vez de frases.
  - As domain notifications existem, mas o `MemoryBus.RaiseEvent` está comentado, então nenhuma notificação chega ao `DomainNotificationHandler`.
  - `OrderItem.ProductId` é `Guid`, tanto no domínio quanto na configuração do EF.
  - Não há CORS. O Catalog já tem o padrão: `Cors:AllowedOrigins` no `appsettings.json` → `AddCors` com default policy → `UseCors()`.

## Goals / Non-Goals

**Goals:**
- Fazer o endpoint devolver um resultado tipado (sucesso ou erros) sem depender do mecanismo de notificações, que hoje está desligado.
- Manter o front-end sem novas dependências: validação manual e `fetch`, como em `catalog.ts`.

**Non-Goals:**
- Religar `RaiseEvent` e as domain notifications.
- Persistir os dados do cartão. O comportamento atual (`PaymentMethod` nulo) é mantido e os dados só trafegam no comando. Guardar número e CVV sem tokenização seria um problema maior que a lacuna atual.
- Traduzir os códigos de validação do domínio.

## Decisions

### 1. Comandos com resposta no `IMemoryBus`
Criar `Message<TResponse> : Message, IRequest<TResponse>` em `Shared.Code` e adicionar `Task<TResponse> RequestAsync<TResponse>(Message<TResponse> command)` ao `IMemoryBus`/`MemoryBus`, que repassa para `_mediator.Send((IRequest<TResponse>)command)`. O nome é diferente de `SendAsync` porque uma sobrecarga `SendAsync<TResponse>` perderia, na resolução de sobrecarga, para o `SendAsync<T>(T) where T : Message` existente (a inferência casa com o tipo exato do comando), e a resposta seria descartada sem erro. Pelo mesmo motivo o cast escolhe o `Send(IRequest<TResponse>)` do MediatR. O `CreateCommand` passa a herdar `Message<CreateOrderResult>`, e o `OrderHandler` implementa `IRequestHandler<CreateCommand, CreateOrderResult>`.

- *Alternativa:* religar `RaiseEvent` e ler o `DomainNotificationHandler` (scoped) no endpoint. Seria fiel ao padrão do projeto, mas mexe no bus inteiro, que também é usado para eventos e event store. Além disso, não resolve a devolução do número do pedido.
- *Alternativa:* usar `Result<T>` do `Programming.Functional`. Isso exigiria uma referência nova no Orders, e esse projeto ainda é net6.0.
- *Risco:* o record herda o `IRequest` não genérico (via `Message`) e o `IRequest<T>`. O `MemoryBus` chama a sobrecarga genérica de forma explícita. Se o scan do MediatR reclamar, `Message<TResponse>` passa a ser um record base independente, com os mesmos `MessageType`/`AggregateId`.

### 2. Validação no handler
O handler monta o `Order`, chama `IsValid()` e só salva se for válido. `CreateOrderResult` é um record com `Success`, `OrderNumber`, `Status` (`Order.Status.Name`), `Total` e `Errors` (`Order.Errors`). O endpoint mapeia o resultado: sucesso vira `Results.Created($"/orders/{OrderNumber}", body)` e falha vira `Results.BadRequest(new { errors })`.

No `OrderValidation`, sai a regra de `PaymentId`, porque o pedido nasce `Pending`. Ela volta quando a change de pagamento existir, condicionada a status ≠ `Pending`. Entra `RuleFor(x => x.Items).NotEmpty()`.

### 3. `ProductId` como `int`
A troca de `Guid` para `int` atravessa `OrderItemMessageResponse`, `OrderBuilder.CreateItem`, `OrderItem` (incluindo `RemoveItem`) e `OrderItemConfigurations`. O Catalog é a fonte do id e usa `int`. Converter no front-end inventaria um Guid sem significado.

### 4. CORS igual ao do Catalog
Usar a mesma chave `Cors:AllowedOrigins` (`http://localhost:3000`) e a mesma default policy, com `UseCors()` antes dos endpoints. Manter o `UseHttpsRedirection`, como no Catalog. Em `dotnet run` e no compose a API é chamada por HTTP no profile http.

### 5. Front-end
- `src/lib/orders.ts`: `createOrder(request)` faz `POST ${VITE_ORDERS_API_URL ?? "http://localhost:5224"}/createOrder`. Ele devolve `{ ok: true, order }` no 201, `{ ok: false, errors }` no 400 e lança erro em falha de rede ou outros status. A variável entra no `Dockerfile` (ARG/ENV) e no `docker-compose.frontend.yml`, como a do Catalog.
- `src/lib/customer.ts`: `getCustomerId()` lê ou grava `crypto.randomUUID()` no `localStorage` (chave `customerId`).
- Página `src/pages/CheckoutPage.tsx` com componentes em `src/components/checkout/`: `CheckoutItems`, `ShippingAddressForm`, `PaymentForm`, `CheckoutSummary` e `OrderConfirmation`. O estado do formulário fica na página, em `useState`, e a validação é uma função pura que devolve erros por campo. A página não usa `useRequest`, porque o envio é uma ação do usuário e não um carregamento.
- O sucesso troca o conteúdo da página pela confirmação e chama `clearCart()`. A confirmação é renderizada a partir do resultado guardado no estado, para que o redirecionamento de carrinho vazio não a esconda.
- O `Amount` do cartão recebe o total calculado no front-end. O `OrderId` (um `Guid`) vai como `00000000-0000-0000-0000-000000000000`, porque o servidor ainda não o usa.
- `OrderSummary` perde as linhas de frete e imposto e o aviso de "free shipping". O total passa a ser o subtotal.

## Risks / Trade-offs

- [O banco existente tem `ProductId` como `uuid`] → `EnsureCreated` não altera tabelas. Documentar no `CLAUDE.md`/tasks que é preciso recriar o volume do `orders-db` (`docker compose down -v` ou remover só o volume).
- [Preço e total vêm do `localStorage` e podem ser adulterados] → aceito para estudo. O recálculo pelo Catalog está fora do escopo (proposal).
- [As mensagens de erro são códigos como `InvalidOrderAddressCityEmpty`] → o front-end valida antes de enviar, então elas só aparecem se as validações divergirem. O front-end exibe a lista como veio.
- [O caminho de persistência nunca foi exercitado, por exemplo a shadow property `Units` em `OrderItemConfigurations`] → uma task de verificação ponta a ponta pelo `.http`/Swagger antes de ligar o front-end.

## Migration Plan

1. Fazer o deploy do Orders com o banco recriado.
2. Fazer o deploy do front-end com `VITE_ORDERS_API_URL`.

O rollback é reverter as duas imagens. Não há dados de pedidos reais a preservar.

## Open Questions

- Traduzir os códigos de validação para frases legíveis no front-end? Pode ficar para depois sem afetar o contrato.
