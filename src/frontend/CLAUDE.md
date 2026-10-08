# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão geral

Front-end da loja Scorponok, feito em React 19 + Vite (SPA, sem servidor Node em produção), TypeScript, React Router 7, Tailwind CSS v4 e componentes shadcn/ui. Ele parte do template **BloomShop** (ThemeWagon), que era em Next.js, por isso o `package.json` ainda se chama `bloom-ecommerce-template` e os textos da UI estão em inglês. O backend .NET fica em `../backend` (veja o `CLAUDE.md` da raiz do repositório). Os produtos vêm do `Catalog.Web.Api`, que precisa estar no ar: sem ele, a home e as páginas de produto mostram o `ErrorState`.

## Comandos

```bash
npm install
npm run dev      # servidor de desenvolvimento em http://localhost:3000
npm run build    # checagem de tipos (tsc) + build de produção em dist/
npm run preview  # serve o dist/ em http://localhost:3000
npm run lint     # lint (eslint .)
```

As URLs das APIs vêm de `VITE_CATALOG_API_URL` (padrão `http://localhost:5064`) e `VITE_ORDERS_API_URL` (padrão `http://localhost:5224`), as portas do `dotnet run` e do compose. Elas são embutidas no JavaScript durante o build e usadas pelo navegador, então precisam ser endereços que o browser alcance (nunca o nome de um container). No Docker, o serviço `frontend` fica em `docker-compose.frontend.yml` na raiz do repositório (incluído pelo `docker-compose.yml`) e sobe em http://localhost:3000. A imagem vem do `Dockerfile` desta pasta: faz o build com Node e serve o `dist/` com nginx (`nginx.conf`, com fallback para `index.html` por ser SPA). Para mudar as URLs no Docker, use os build args `VITE_CATALOG_API_URL` e `VITE_ORDERS_API_URL` (definidos no `docker-compose.frontend.yml`).

- O dev server e o container usam a porta 3000 porque `http://localhost:3000` é a origem liberada no CORS do `Catalog.Web.Api` e do `Orders.Web.Api` (`Cors:AllowedOrigins` no `appsettings.json` de cada um). Outra porta ou origem precisa ser adicionada nos dois.
- Não há testes nem framework de testes configurado.

## Arquitetura

- **Entrada**: `index.html` (título, meta e fonte Inter via Google Fonts) carrega `src/main.tsx`, que monta `<BrowserRouter>` e `src/App.tsx`. O `App` tem o layout (`CartProvider`, `Header`, `Footer`) e as rotas.
- **Dados**: `src/lib/catalog.ts` é o cliente do `Catalog.Web.Api` (`getProducts` → `GET /Products`, paginado com no máximo 20 itens por página; `getProduct` → `GET /GetProductById/{id}`, `null` no 404). Ele converte `pictureUri` da API em `image`. As chamadas saem do navegador e aparecem na aba Network. Os componentes as fazem via `useRequest(key, load)` (`src/hooks/useRequest.ts`), que devolve `loading`/`data`/`error`/`retry`, refaz a chamada quando `key` muda e cancela a anterior com `AbortController`. Em dev, o `StrictMode` monta tudo duas vezes, então a primeira chamada aparece como cancelada no Network. Os produtos vêm do seed do backend (`Catalog.Infrastructure/Seeds/CatalogDbContextSeed.cs`), com imagens do Unsplash usadas direto em `<img>`. `src/lib/orders.ts` é o cliente do `Orders.Web.Api`: `createOrder` faz `POST /createOrder` e devolve `{ ok: true, order }` no 201 ou `{ ok: false, errors }` no 400; falha de rede e outros status viram exceção. O cliente do pedido é um GUID anônimo de `getCustomerId()` (`src/lib/customer.ts`), guardado no `localStorage` (chave `customerId`).
- **Carrinho**: `src/context/CartContext.tsx` lê e grava o carrinho no `localStorage` (chave `cart`) via `useSyncExternalStore`. O `CartProvider` envolve toda a aplicação em `src/App.tsx`, e os componentes o acessam com `useCart()`. O `addToCart` sempre soma 1 à quantidade, então a página de produto chama a função em loop para adicionar N unidades.
- **Rotas** (`src/App.tsx`, páginas em `src/pages/`): `/` (`HomePage`, lista paginada com 18 produtos por página; a página atual fica na query string `?page=N`, lida em `ProductList` e navegada pelo `ProductPagination`), `/product/:productId` (`ProductPage`), `/cart`, `/checkout` (`CheckoutPage`), `/contact` e `*` (`NotFoundPage`). Use `Link`/`useNavigate`/`useLocation` de `react-router`.
- **Checkout**: a `CheckoutPage` redireciona para `/cart` se o carrinho estiver vazio e guarda o formulário em `useState`. Os blocos ficam em `src/components/checkout/` (`CheckoutItems`, `ShippingAddressForm`, `PaymentForm`, `CheckoutSummary`, `OrderConfirmation`). A validação é a função pura `validateCheckout` em `src/lib/checkout.ts`, que roda antes do envio. No sucesso, a página chama `clearCart()` e mostra o número do pedido; no erro, mantém carrinho e formulário. O resumo do carrinho (`OrderSummary`) e o do checkout mostram total = soma dos itens, sem frete nem imposto, igual ao `Total` que o Orders registra.
- **Componentes**: `src/components/ui/` contém os primitivos do shadcn (estilo `new-york`, ícones `lucide-react`, configurados em `components.json`). As outras pastas de `src/components/` são organizadas por página (`home`, `product`, `cart`, `checkout`, `layout`); `ErrorState` e `LoadingState` ficam na raiz. Use `cn()` de `src/lib/utils.ts` para combinar classes. O import `@/*` aponta para `src/` (`tsconfig.json` e `vite.config.ts`).
- **Estilo**: o Tailwind v4 entra pelo plugin `@tailwindcss/vite` e é configurado via CSS em `src/index.css` (`@import "tailwindcss"`, tokens de tema como variáveis CSS em `oklch`, variante `dark` via classe `.dark`). Use os tokens semânticos (`bg-background`, `text-primary`, `text-muted-foreground` etc.) em vez de cores fixas.
