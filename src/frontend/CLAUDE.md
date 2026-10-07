# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão geral

Front-end da loja Scorponok, feito em Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4 e componentes shadcn/ui. Ele parte do template **BloomShop** (ThemeWagon), por isso o `package.json` ainda se chama `bloom-ecommerce-template` e os textos da UI estão em inglês. O backend .NET fica em `../backend` (veja o `CLAUDE.md` da raiz do repositório). Por enquanto o front-end **não chama nenhuma API**: os produtos vêm de um JSON estático.

## Comandos

```bash
npm install
npm run dev      # servidor de desenvolvimento em http://localhost:3000
npm run build    # build de produção (também faz a checagem de tipos)
npm run start    # serve o build
npm run lint     # lint (eslint .)
```

No Docker, o serviço `frontend` fica em `docker-compose.frontend.yml` na raiz do repositório (incluído pelo `docker-compose.yml`) e sobe em http://localhost:3000. A imagem vem do `Dockerfile` desta pasta e usa o build `output: "standalone"` do `next.config.ts`; não remova essa opção.

- Não há testes nem framework de testes configurado.

## Arquitetura

- **Dados**: `data/products.json` é a única fonte de produtos. `ProductList`, `RelatedProducts` e `app/product/[productId]/page.tsx` importam o JSON direto. O tipo fica em `types/product.ts`. As imagens são URLs do Unsplash, que precisam estar liberadas em `images.remotePatterns` no `next.config.ts`. Para integrar com o backend, a fonte a trocar é o endpoint `GET /GetAllProducts` (paginado) do `Catalog.Web.Api` e, para o checkout, o `POST /createOrder` do `Orders.Web.Api`. Nenhuma das APIs configura CORS ainda.
- **Carrinho**: `context/CartContext.tsx` é um Client Component que guarda o carrinho em estado React e o persiste no `localStorage` (chave `cart`). O `CartProvider` envolve toda a aplicação em `app/layout.tsx`, e os componentes o acessam com `useCart()`. O `addToCart` sempre soma 1 à quantidade, então a página de produto chama a função em loop para adicionar N unidades.
- **Rotas** (`app/`): `/` (lista de produtos), `/product/[productId]`, `/cart` e `/contact`. O botão de checkout em `OrderSummary` aponta para `/checkout`, uma rota que ainda não existe.
- **Componentes**: `components/ui/` contém os primitivos do shadcn (estilo `new-york`, ícones `lucide-react`, configurados em `components.json`). As outras pastas de `components/` são organizadas por página (`home`, `product`, `cart`, `layout`). Use `cn()` de `lib/utils.ts` para combinar classes. O import `@/*` aponta para a raiz deste diretório.
- **Estilo**: o Tailwind v4 é configurado via CSS em `app/globals.css` (`@import "tailwindcss"`, tokens de tema como variáveis CSS em `oklch`, variante `dark` via classe `.dark`). Use os tokens semânticos (`bg-background`, `text-primary`, `text-muted-foreground` etc.) em vez de cores fixas. O `tailwind.config.js` é legado do v3 e praticamente não tem efeito.
