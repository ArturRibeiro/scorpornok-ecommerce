# Design

## Context

- O cliente é hoje um GUID anônimo (`src/frontend/src/lib/customer.ts`, chave `customerId` no `localStorage`) enviado como `userId` no `POST /createOrder`, junto com o `email` digitado no `ContactForm`. O `CreateCommand` recebe os dois do corpo e o `Order` guarda `CustomerId` (Guid) e `Email`.
- O hub `OrderHub.WatchOrder(orderNumber)` aceita qualquer conexão de uma origem liberada e consulta só o status (`GetOrderStatusQuery`), sem saber de quem é o pedido.
- O e-mail do resultado já sai pelo `OrderEmailDispatcher` para `Order.Email`; o Mailpit já está no compose.
- Nenhuma API tem autenticação. O front chama as APIs direto do navegador (CORS com `AllowCredentials` no Orders).

## Goals / Non-Goals

**Goals:**
- O navegador e o `orders-api` (dentro do compose) validam o mesmo token, embora alcancem o Keycloak por endereços diferentes.
- O realm é reproduzível: subir o compose do zero gera o realm, o client e as regras sem passo manual.
- A regra "quem é o cliente" fica num só lugar no Orders (os claims do token), testável sem subir o Keycloak.

**Non-Goals:**
- Guardar usuários ou senhas nos nossos bancos.
- Autorização por papéis.
- Mudar o modelo de dados do Orders.

## Decisions

### 1. Formulário do Keycloak com Authorization Code + PKCE
O front redireciona para a página de login/cadastro do Keycloak e recebe o código em `/auth/callback`. Client público `scorponok-web`, sem segredo, PKCE `S256`.
- *Alternativa*: formulário React próprio com o fluxo de senha direta (Direct Access Grants). Descartada: o fluxo está obsoleto no OAuth 2.1, expõe a senha ao front e perde de graça o cadastro, a confirmação de e-mail, o reenvio e a proteção contra força bruta do Keycloak.

### 2. Keycloak no compose, com banco próprio e realm importado
`docker-compose.identity.yml` (incluído pelo `docker-compose.yml`) com `keycloak` (imagem `quay.io/keycloak/keycloak`, 26.x, `start-dev --import-realm`, porta 8080) e `keycloak-db` (Postgres 16, porta 5436 no host), no padrão de um banco por serviço. O admin inicial vem de `KC_BOOTSTRAP_ADMIN_USERNAME`/`KC_BOOTSTRAP_ADMIN_PASSWORD` (padrão local `admin`/`admin`). Desde a versão 25 os health checks ficam na porta de gerenciamento 9000 (`KC_HEALTH_ENABLED=true`), e a imagem não tem `curl`: o healthcheck do compose consulta `localhost:9000/health/ready` pelo `/dev/tcp` do bash. O realm fica em `keycloak/scorponok-realm.json`, montado em `/opt/keycloak/data/import`.
Configuração do realm `scorponok`: `registrationAllowed`, `registrationEmailAsUsername`, `loginWithEmailAllowed`, `duplicateEmailsAllowed=false`, `verifyEmail`, `resetPasswordAllowed`, `bruteForceProtected`, política de senha `length(8)`, SMTP `mailpit:1025` com remetente `no-reply@scorponok.local`.
- *Alternativa*: configurar o realm por script na partida (admin REST/`kcadm`). Descartada: o import é declarativo e versionado. O custo é que o import só roda quando o realm não existe, então mudar o arquivo exige recriar o volume do `keycloak-db`.

### 3. Mesmo emissor para navegador e containers
O navegador usa `http://localhost:8080`, e o `orders-api` alcança o Keycloak por `http://keycloak:8080`. O Keycloak recebe `KC_HOSTNAME=http://localhost:8080` e `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true`: o `iss` dos tokens é sempre `http://localhost:8080/realms/scorponok`, e a descoberta pedida pela rede interna devolve as URLs de chaves no endereço interno. No Orders, a seção `Keycloak` tem `Authority` (emissor público, validado em `ValidIssuer`) e `MetadataAddress` (descoberta pelo endereço alcançável). No `dotnet run` os dois apontam para `localhost:8080`.
- *Alternativa*: `extra_hosts` mapeando `localhost` para o Keycloak dentro do container. Descartada: frágil e conflita com o próprio `localhost` do container.

### 4. Audiência própria para o Orders
Um client scope `orders-api` com *audience mapper* do tipo "Included Custom Audience" (valor `orders-api`, sem precisar de um client com esse nome) entra como escopo padrão do `scorponok-web`; o Orders valida `aud=orders-api`. Assim, um token emitido para outro client do realm não é aceito.
- *Alternativa*: não validar audiência. Descartada: qualquer token do realm passaria.

### 5. Orders: JwtBearer, política e identidade vinda do token
- `AddAuthentication().AddJwtBearer` (pacote `Microsoft.AspNetCore.Authentication.JwtBearer`, versão no `Directory.Build.props`), com `MapInboundClaims=false` para manter `sub` e `email` com esses nomes, `ClockSkew` de 30 s (o padrão de 5 min aceitaria um token vencido por mais tempo que a própria validade de 5 min do access token) e `RequireHttpsMetadata` só fora de Development.
- Política `VerifiedCustomer`: usuário autenticado com `email_verified=true` e com `sub` (GUID) e `email` presentes. Sem token → 401, pelo desafio do JwtBearer. Token válido que falha a política → 403.
- `CreateOrder` e o hub exigem a política. O endpoint lê a identidade com um helper puro (`CustomerIdentity.From(ClaimsPrincipal)` → `CustomerId`, `Email`) e repassa `command with { UserId = id, Email = email }`. O que vier no corpo é sobrescrito, sem mudar o contrato do `CreateCommand` nem os handlers.
- *Alternativa*: tirar `UserId`/`Email` do `CreateCommand` e criar um command novo. Descartada por mexer em handlers e testes sem ganho de comportamento.

### 6. Hub: token na query string e dono do pedido
O WebSocket não envia cabeçalho `Authorization`, então o `OnMessageReceived` do JwtBearer lê `access_token` da query quando o caminho começa com `/hubs/orders`. No front, `accessTokenFactory` entrega o token. O `GetOrderStatusQuery` passa a receber o `CustomerId` e devolve nulo quando o pedido não existe ou é de outro cliente. O `WatchOrder` então lança `HubException` antes de entrar no grupo, com a mesma mensagem nos dois casos, para não revelar que o número existe.

### 7. Front: `oidc-client-ts` num `AuthProvider`
`UserManager` com `WebStorageStateStore(localStorage)` (sessão compartilhada entre abas, como o carrinho), escopo `openid email`, `automaticSilentRenew` por refresh token. Partes:
- `AuthProvider` e `useAuth()` (`user`, `email`, `login(returnTo)`, `register(returnTo)`, `logout()`, `getAccessToken()`).
- Rota `/auth/callback`, que conclui o login e navega para o `returnTo` guardado no `state`.
- `RequireAuth` em volta de `/checkout`, que chama `login("/checkout")` quando não há sessão. A checagem de carrinho vazio da `CheckoutPage` vem antes, para não mandar ao login quem seria devolvido ao carrinho.
- Cadastro com `prompt=create`, que abre direto a tela de registro do Keycloak 26.

O carrinho sobrevive aos redirecionamentos porque já está no `localStorage`. O header mostra "Sign in"/"Create account" ou o e-mail e "Sign out" (`signoutRedirect` com retorno à home). URL por `VITE_KEYCLOAK_URL` (padrão `http://localhost:8080`), realm e client fixos em constantes.
- *Alternativa*: `keycloak-js`. Também serviria; `oidc-client-ts` é agnóstico de provedor e já tipado.

### 8. Testes
- Unitários no `Orders.Tests` (passa a referenciar o `Orders.Web.Api`): `CustomerIdentity.From` com claims completos, sem `sub`, `sub` não-GUID e sem `email`; a política com `email_verified` false/ausente; o `GetOrderStatusQuery` com dono certo, outro dono e pedido inexistente; o `WatchOrder` recusando pedido alheio.
- Os códigos 401/403/201 de ponta a ponta são verificados no compose com o `.http` e o roteiro manual. Um `WebApplicationFactory` do Orders exigiria Postgres, porque a partida faz `EnsureCreated` e seed.

## Risks / Trade-offs

- [Keycloak fora do ar] → login e cadastro param. O Orders guarda em cache as chaves já baixadas e continua validando tokens emitidos antes; `/health/ready` do Orders não passa a depender do Keycloak.
- [Mudança no `scorponok-realm.json` não aplicada num volume existente] → documentar no `CLAUDE.md` a recriação do volume do `keycloak-db`, como já é feito para os bancos sem migrations.
- [Token no `localStorage` exposto a XSS] → aceitável no escopo de estudo, com access token curto (5 min, padrão) e renovação silenciosa. Em produção, avaliar um BFF com cookie `HttpOnly`.
- [Token na query string do WebSocket aparece em logs] → limitar ao caminho do hub e não logar query strings.
- [O cadastro revela se o e-mail já existe] → é o comportamento padrão do Keycloak ("Email already exists") e o que o requisito pede (oferecer o login). O login continua sem revelar: a mensagem é a mesma para e-mail inexistente e senha errada.
- [403 inalcançável pelo fluxo normal] → com `verifyEmail` o Keycloak não emite token antes da confirmação, então a política de `email_verified` é defesa em profundidade (ex.: alguém desliga o `verifyEmail` no realm). Ela é coberta por teste unitário, não pelo roteiro manual.
- [Link de confirmação aberto em outra aba ou aparelho] → o Keycloak pode terminar na página "e-mail confirmado" em vez de voltar à loja. O cliente então entra pelo checkout e volta a ele normalmente, porque o carrinho está no `localStorage`; o roteiro manual (6.2) cobre os dois caminhos.
- [Dados de entrega e cartão perdidos ao reautenticar] → o redirecionamento sai da SPA e o formulário vive em `useState`. Só o carrinho é preservado; os dados do cartão não devem ir para o `localStorage`.
- [Primeira partida lenta do Keycloak] → `healthcheck` no serviço e `depends_on: condition: service_healthy` no `orders-api`.

## Migration Plan

1. `docker compose up -d --build`: sobe `keycloak-db` e `keycloak`, importa o realm e atualiza `orders-api` e `frontend`. Os bancos existentes não mudam.
2. Pedidos anônimos já gravados continuam no `orders-db` com o `CustomerId` antigo; nenhuma conta os enxerga.
3. Rollback: reverter o commit e subir de novo. O Keycloak e o volume dele podem ser removidos sem afetar os outros serviços.
