# Proposal

## Why

O carrinho nunca vira pedido. O botão "Proceed to Checkout" leva para `/checkout`, uma rota que não existe. O `POST /createOrder` do Orders não é chamado por ninguém e também não serviria ao front-end: não devolve número do pedido nem erros, espera `ProductId` como `Guid` (o Catalog usa `int`), ignora o `UserId` e não libera CORS. Esta change fecha o fluxo carrinho → pedido do cenário 1 do `use-case.md`.

## What Changes

- **Página de checkout** (`/checkout`) com quatro blocos:
  1. Revisão dos itens do carrinho (só leitura, com link para editar o carrinho).
  2. Endereço de entrega: rua, cidade, estado, país e CEP.
  3. Pagamento com cartão: titular, número, validade, CVV e parcelas.
  4. Resumo com o total e o botão "Place order", com estados de envio, erro e sucesso.
- O checkout com o carrinho vazio redireciona para `/cart`.
- O pedido vai para o `Orders.Web.Api`. No sucesso, mostra o número do pedido e esvazia o carrinho. No erro, mostra as mensagens e mantém o carrinho e o formulário.
- O cliente é identificado por um GUID anônimo, gerado uma vez e guardado no navegador.
- **Frete e imposto saem da tela** no carrinho e no checkout. O total exibido passa a ser a soma dos itens, igual ao `Total` que o Orders registra.
- **Orders.Web.Api** (ajustes mínimos):
  - Libera CORS para a origem do front-end.
  - **BREAKING**: `ProductId` dos itens passa de `Guid` para `int`, alinhado ao Catalog.
  - Usa o `UserId` recebido como `CustomerId` do pedido.
  - Valida o pedido antes de salvar e responde `201` com número, status e total, ou `400` com as mensagens de validação.
  - A criação deixa de exigir `PaymentId`, porque o pedido nasce `Pending`, antes de qualquer pagamento.

## Capabilities

### New Capabilities
- `checkout`: a experiência de finalizar a compra no front-end. Cobre revisão dos itens, dados de entrega e pagamento, total exibido, envio do pedido e o resultado para o cliente.
- `order-placement`: o contrato da API de Orders para registrar um pedido. Cobre entrada aceita, validação, resposta de sucesso e de erro, e acesso a partir do navegador.

### Modified Capabilities
Nenhuma.

## Impact

- **Front-end**: rota e página de checkout, cliente da API de Orders, variável `VITE_ORDERS_API_URL` (`Dockerfile` e compose), `OrderSummary` sem frete e imposto.
- **Orders**: CORS, resposta do endpoint, `CreateCommand`, handler e builder, `OrderItem` e sua configuração no EF, `OrderValidation` e o arquivo `.http`.
- O banco do Orders precisa ser recriado, porque o tipo da coluna `ProductId` muda e não há migrations.

## Fora do escopo

- Cobrança real: chamar o gateway, mudar o status para `Confirmed`/`Failed` e preencher `PaymentId`. O pedido fica `Pending`.
- Recalcular preços no servidor a partir do Catalog. O preço ainda vem do carrinho.
- Frete, impostos, descontos e cupons no domínio.
- Login ou cadastro de clientes, e consulta ou histórico de pedidos.
- Tokenização ou criptografia dos dados do cartão.
- Framework de testes no front-end.
