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

A URL da API vem de `VITE_CATALOG_API_URL` (padrão `http://localhost:5064`, a porta do `dotnet run` e do compose). Ela é embutida no JavaScript durante o build e usada pelo navegador, então precisa ser um endereço que o browser alcance (nunca o nome de um container). No Docker, o serviço `frontend` fica em `docker-compose.frontend.yml` na raiz do repositório (incluído pelo `docker-compose.yml`) e sobe em http://localhost:3000. A imagem vem do `Dockerfile` desta pasta: faz o build com Node e serve o `dist/` com nginx (`nginx.conf`, com fallback para `index.html` por ser SPA). Para mudar a URL da API no Docker, use o build arg `VITE_CATALOG_API_URL`.

- O dev server e o container usam a porta 3000 porque `http://localhost:3000` é a origem liberada no CORS do `Catalog.Web.Api` (`Cors:AllowedOrigins` no `appsettings.json`). Outra porta ou origem precisa ser adicionada lá.
- Não há testes nem framework de testes configurado.

## Arquitetura

- **Entrada**: `index.html` (título, meta e fonte Inter via Google Fonts) carrega `src/main.tsx`, que monta `<BrowserRouter>` e `src/App.tsx`. O `App` tem o layout (`CartProvider`, `Header`, `Footer`) e as rotas.
- **Dados**: `src/lib/catalog.ts` é o cliente do `Catalog.Web.Api` (`getProducts` → `GET /GetAllProducts`, paginado com no máximo 20 itens por página; `getProduct` → `GET /GetProductById/{id}`, `null` no 404). Ele converte `pictureUri` da API em `image`. As chamadas saem do navegador e aparecem na aba Network. Os componentes as fazem via `useRequest(key, load)` (`src/hooks/useRequest.ts`), que devolve `loading`/`data`/`error`/`retry`, refaz a chamada quando `key` muda e cancela a anterior com `AbortController`. Em dev, o `StrictMode` monta tudo duas vezes, então a primeira chamada aparece como cancelada no Network. Os produtos vêm do seed do backend (`Catalog.Infrastructure/Seeds/ApplicationCatalogDbContextSeed.cs`), com imagens do Unsplash usadas direto em `<img>`. O checkout ainda não chama o `POST /createOrder` do `Orders.Web.Api`.
- **Carrinho**: `src/context/CartContext.tsx` lê e grava o carrinho no `localStorage` (chave `cart`) via `useSyncExternalStore`. O `CartProvider` envolve toda a aplicação em `src/App.tsx`, e os componentes o acessam com `useCart()`. O `addToCart` sempre soma 1 à quantidade, então a página de produto chama a função em loop para adicionar N unidades.
- **Rotas** (`src/App.tsx`, páginas em `src/pages/`): `/` (`HomePage`), `/product/:productId` (`ProductPage`), `/cart`, `/contact` e `*` (`NotFoundPage`). O botão de checkout em `OrderSummary` aponta para `/checkout`, uma rota que ainda não existe e cai no `NotFoundPage`. Use `Link`/`useNavigate`/`useLocation` de `react-router`.
- **Componentes**: `src/components/ui/` contém os primitivos do shadcn (estilo `new-york`, ícones `lucide-react`, configurados em `components.json`). As outras pastas de `src/components/` são organizadas por página (`home`, `product`, `cart`, `layout`); `ErrorState` e `LoadingState` ficam na raiz. Use `cn()` de `src/lib/utils.ts` para combinar classes. O import `@/*` aponta para `src/` (`tsconfig.json` e `vite.config.ts`).
- **Estilo**: o Tailwind v4 entra pelo plugin `@tailwindcss/vite` e é configurado via CSS em `src/index.css` (`@import "tailwindcss"`, tokens de tema como variáveis CSS em `oklch`, variante `dark` via classe `.dark`). Use os tokens semânticos (`bg-background`, `text-primary`, `text-muted-foreground` etc.) em vez de cores fixas.
