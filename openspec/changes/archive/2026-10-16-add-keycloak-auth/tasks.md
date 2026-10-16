# Tasks

## 1. Keycloak no compose

- [x] 1.1 Criar `docker-compose.identity.yml` com `keycloak-db` (Postgres 16, porta 5436, volume próprio, healthcheck) e `keycloak` (26.x, `start-dev --import-realm`, porta 8080, `KC_DB=postgres`, `KC_HOSTNAME=http://localhost:8080`, `KC_HOSTNAME_BACKCHANNEL_DYNAMIC=true`, `KC_HEALTH_ENABLED=true` e healthcheck em `localhost:9000/health/ready` via `/dev/tcp` (a imagem não tem `curl`), admin por `KC_BOOTSTRAP_ADMIN_USERNAME`/`KC_BOOTSTRAP_ADMIN_PASSWORD` com padrão local) e incluí-lo no `docker-compose.yml`; verificar com `docker compose -f docker-compose.identity.yml up -d` e o console em http://localhost:8080 abrindo
- [x] 1.2 Criar `keycloak/scorponok-realm.json` com o realm `scorponok` (cadastro, e-mail como usuário, login por e-mail, sem e-mails duplicados, `verifyEmail`, `resetPasswordAllowed`, `bruteForceProtected`, política `length(8)`, SMTP `mailpit:1025` com remetente `no-reply@scorponok.local`), o client público `scorponok-web` (Standard Flow, PKCE `S256`, redirect `http://localhost:3000/*`, web origin e post-logout `http://localhost:3000`) com o audience mapper `orders-api` no próprio client; montar em `/opt/keycloak/data/import`; verificar recriando o volume do `keycloak-db`: o realm aparece importado e a tela de registro de `http://localhost:8080/realms/scorponok/account` oferece cadastro
- [x] 1.3 Verificar o cadastro no Keycloak sem a loja: cadastrar cliente@exemplo.com, ver o e-mail de confirmação em http://localhost:8025, abrir o link, definir a senha (7 caracteres recusada, 8 aceita) e entrar; login antes de confirmar falha com a mensagem genérica; cadastrar o mesmo e-mail é recusado; um link perdido é reenviado pela recuperação de senha
- [x] 1.4 Documentar no `CLAUDE.md` da raiz o serviço `keycloak` (portas 8080/5436, console admin, realm versionado, recriação do volume para reimportar o realm) e o comando `docker compose -f docker-compose.identity.yml up -d`; verificar que os comandos documentados rodam como escritos

## 2. Autenticação no Orders

- [x] 2.1 Adicionar `$(AspNetCoreAuthenticationJwtBearer)` ao `Directory.Build.props` e a referência no `Orders.Web.Api.csproj`; seção `Keycloak` (`Authority`, `MetadataAddress`, `Audience`) no `appsettings.json` e no `appsettings.Development.json` (`localhost:8080`) e variáveis no `orders-api` do compose (`MetadataAddress` em `keycloak:8080`, `depends_on` com `service_healthy`); verificar com `dotnet build Scorponok.slnx`
- [x] 2.2 Criar `CustomerIdentity.From(ClaimsPrincipal)` (`sub` como GUID e `email`) e a política `VerifiedCustomer` (autenticado, `email_verified=true`, `sub` GUID e `email` presentes); configurar `AddJwtBearer` com `MapInboundClaims=false`, `ValidIssuer`, `ValidAudience`, `ClockSkew` de 30 s e `RequireHttpsMetadata` fora de Development; adicionar `UseAuthentication`/`UseAuthorization`; verificar com testes no `Orders.Tests` (referenciando `Orders.Web.Api`): identidade completa, sem `sub`, `sub` não-GUID, sem `email`; política aprova com `email_verified=true` e reprova com `false` ou ausente
- [x] 2.3 Exigir a política no `CreateOrder` e repassar `command with { UserId, Email }` vindos da identidade; verificar com teste do mapeamento: o corpo traz `userId` D e `outro@exemplo.com`, e o command enviado ao bus sai com o `sub` e o e-mail do token
- [x] 2.4 Atualizar o `.http` do Orders (como obter um token pelo Keycloak e o cabeçalho `Authorization` no pedido válido) e verificar no compose: sem token → `401`; token de conta confirmada → `201` com o pedido gravado com o `sub` e o e-mail do token; token expirado (esperar cerca de 5,5 min depois de emitido) → `401`. O `403` não é reproduzível pelo fluxo normal (o Keycloak não emite token sem e-mail confirmado) e fica coberto pelo teste da política em 2.2

## 3. Dono do pedido no hub

- [x] 3.1 Fazer o `GetOrderStatusQuery` receber o `CustomerId` e devolver nulo para pedido inexistente ou de outro cliente; verificar com testes no `OrderPaymentHandlersTests`: dono certo devolve o status, outro cliente e número inexistente devolvem nulo
- [x] 3.2 Exigir a política no `OrderHub`, ler `access_token` da query no `OnMessageReceived` só para `/hubs/orders`, e fazer o `WatchOrder` usar o `CustomerId` da identidade e lançar `HubException` ("Order not found.") antes de entrar no grupo quando a consulta devolver nulo; verificar com teste do hub (bus e contexto mockados): pedido alheio lança a exceção e não entra no grupo, pedido próprio já resolvido envia o resultado
- [x] 3.3 Atualizar no `CLAUDE.md` da raiz a descrição do `POST /createOrder` (identidade do token, 401/403, campos `userId`/`email` do corpo ignorados), do hub (token e dono) e da seção `Keycloak`; verificar relendo contra o código

## 4. Login e cadastro na loja

- [x] 4.1 Adicionar `oidc-client-ts` ao `src/frontend/package.json` e criar `src/lib/auth.ts` (`UserManager` com `VITE_KEYCLOAK_URL`, realm `scorponok`, client `scorponok-web`, escopo `openid email`, `localStorage`, renovação silenciosa) e `src/context/AuthContext.tsx` (`AuthProvider` e `useAuth()` com `user`, `email`, `login(returnTo)`, `register(returnTo)` com `prompt=create`, `logout()`, `getAccessToken()`); verificar com `npm run build && npm run lint`
- [x] 4.2 Criar a rota `/auth/callback` (conclui o login e navega para o `returnTo`) e o `RequireAuth` em volta de `/checkout`, depois da checagem de carrinho vazio; envolver o app no `AuthProvider`; verificar com o compose: carrinho com 2 itens → "Proceed to Checkout" sem login leva ao Keycloak e, depois de entrar, volta ao checkout com os 2 itens; carrinho vazio sem login em `/checkout` vai para `/cart` sem passar pelo login
- [x] 4.3 Mostrar no `Header` "Sign In" e "Sign Up" (botões que já existiam no template) sem sessão, e o e-mail com "Sign out" com sessão; verificar no navegador: cadastro a partir do header abre a tela de registro, e "Sign out" encerra a sessão e o checkout volta a pedir login
- [x] 4.4 Documentar no `CLAUDE.md` do front-end o `AuthProvider`, a rota de callback, o `RequireAuth` e a variável `VITE_KEYCLOAK_URL` (também como build arg no `docker-compose.frontend.yml`); verificar com `docker compose up -d --build frontend` e o login funcionando no container

## 5. Checkout com a conta

- [x] 5.1 Remover o `ContactForm`, a validação de e-mail do `validateCheckout` e o `lib/customer.ts`; exibir o e-mail da conta como destino do resultado; tirar `userId`/`email` do `CreateOrderRequest` e enviar `Authorization: Bearer` no `createOrder`, tratando `401` como sessão expirada (levar ao login mantendo o carrinho); verificar com `npm run build && npm run lint` e no navegador: o checkout mostra o e-mail da conta, sem campo de e-mail
- [x] 5.2 Passar o token ao hub (`accessTokenFactory` no `watchOrderPayment`) e a confirmação usar o e-mail da conta nos textos; verificar no compose: pedido aprovado mostra "Approved" e o e-mail chega no Mailpit para o endereço da conta
- [x] 5.3 Atualizar no `CLAUDE.md` do front-end a seção do checkout (sem `ContactForm` nem `getCustomerId`, e-mail da conta, token no pedido e no hub); verificar relendo contra o código

## 6. Verificação ponta a ponta

- [x] 6.1 Rodar `dotnet test Scorponok.slnx` e `npm run build && npm run lint` em `src/frontend`, todos sem falhas
- [x] 6.2 Roteiro completo no compose, a partir de volumes novos: cadastrar pelo checkout → confirmar o e-mail no Mailpit (abrindo o link numa aba nova, e de novo com outra conta na mesma aba) → entrar → finalizar com cartão aprovado ("Approved" e e-mail de confirmado) → finalizar com cartão terminado em `0000` ("Declined" e e-mail de recusado) → sair e ver o checkout pedir login de novo (a senha é definida depois de abrir o link)
- [x] 6.3 Verificar o isolamento no compose: com a conta B, chamar `WatchOrder` com o número de um pedido da conta A (pelo console do navegador; o `.http` não fala SignalR) e receber "Order not found." sem mensagem de resultado

## Workflow follow-up

- Arquivar a change depois da revisão (`/opsx:archive`), sincronizando as specs `customer-auth`, `checkout` e `order-placement`.
