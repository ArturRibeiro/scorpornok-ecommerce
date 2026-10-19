# Tasks

## 1. Estrutura do contexto Customers

- [x] 1.1 Criar `Customers.Domain`, `Customers.CommandHandlers`, `Customers.Infrastructure`, `Customers.Web.Api` e `Customers.Tests` em `src/backend/Customers/` (net10.0, versões do `Directory.Build.props`) e incluí-los numa nova pasta do `Scorponok.slnx`; verificar com `dotnet build Scorponok.slnx`

## 2. Domínio

- [x] 2.1 Regras compartilhadas `Cpf`, `Phone`, `BrazilianState` e `ZipCode` em `Shared.Code/Validation` (normalização e validação), com testes
- [x] 2.2 Agregado `Customer` (Id = `sub`, `Email`, `FullName`, `Phone`, `Cpf`, `Address` obrigatório) e value object `Address`, com normalização para só dígitos de telefone, CPF e CEP
- [x] 2.3 `CustomerValidator` e `AddressValidator` (nome com 2+ palavras, telefone e CPF pelas regras compartilhadas, UF e CEP pelas regras compartilhadas, todos os campos obrigatórios exceto o complemento), com erros por campo nas chaves do JSON (`cpf`, `address.zipCode`)
- [x] 2.4 Testes do domínio: CPF válido/inválido/todos iguais, pontuação aceita, campos obrigatórios faltando, sem endereço, complemento vazio aceito, UF inválida; verificar com `dotnet test src/backend/Customers/Customers.Tests`

## 3. Persistência e handlers

- [x] 3.1 `CustomerContext` (tabela `Customers`, `Address` como owned type), `ICustomerRepository`/`CustomerRepository` e `AddInfrastructure` com o registro explícito dos handlers; verificar com `dotnet build Scorponok.slnx`
- [x] 3.2 `GetCustomerProfileQuery` (cadastro vazio devolve só o e-mail) e `UpdateCustomerProfileCommand` (upsert, valida antes de gravar, atualiza o e-mail do token), pelo `IMemoryBus.RequestAsync`
- [x] 3.3 Testes dos handlers com repositório mockado: primeiro acesso, upsert que substitui o anterior, erro de validação sem gravar; verificar com `dotnet test src/backend/Customers/Customers.Tests`

## 4. Web API

- [x] 4.1 `Program.cs` com connection string obrigatória, CORS (`Cors:AllowedOrigins`), `EnsureCreatedAsync`, `/health/live` e `/health/ready` (banco) e `public partial class Program`; `appsettings.json`/`appsettings.Development.json` apontando para `localhost:5437`
- [x] 4.2 Autenticação em `Authentication/` (JwtBearer do Keycloak, audience `customers-api`, política `VerifiedCustomer`, `CustomerIdentity`), copiada do Orders sem o tratamento do hub
- [x] 4.3 Endpoints `GET /customers/me` e `PUT /customers/me` em `WebApplicationExtensions/` (`200`, `400 { errors }`, `401`, `403`) e arquivo `Customers.Web.Api.http`; verificar com `dotnet run --project src/backend/Customers/Customers.Web.Api` e o `.http`
- [x] 4.4 Testes da identidade, da política e do mapeamento dos endpoints (cliente vem só do token; outro `sub` não enxerga o cadastro); verificar com `dotnet test src/backend/Customers/Customers.Tests`

## 5. Mensagem da compra

- [x] 5.1 Contrato `OrderPlaced` (com `ShippingAddress`) em `Shared.Code/IntegrationEvents`
- [x] 5.2 Orders: `OrderAddress` com `Number`, `Complement` e `District`, UF e CEP validados pelas regras compartilhadas e `Country` = `Brasil`; novo `address` no `POST /createOrder`, no command e no EF; testes do validador e do mapeamento; verificar com `dotnet test src/backend/Store/Orders.Tests`
- [x] 5.3 Orders: `customer { fullName, phone, cpf }` no `POST /createOrder` e no command, validado com as regras compartilhadas (`400` com as mensagens, como o resto do pedido) e não persistido; testes do validador e do mapeamento; verificar com `dotnet test src/backend/Store/Orders.Tests`
- [x] 5.4 Orders: `OrderHandler` publica `OrderPlaced` no outbox antes do `SaveChanges`, com ou sem cartão; testes do handler (publicado com o endereço do pedido; nada publicado se a gravação falha); verificar com `dotnet test src/backend/Store/Orders.Tests`
- [x] 5.5 Customers: tabela `DeliveryAddresses` (`OrderNumber` com índice único) e `RecordPurchaseCommand` com handler idempotente (histórico sempre; cria o `Customer` só se não existir); testes de primeira compra, cadastro existente inalterado e pedido repetido
- [x] 5.6 Customers: MassTransit (seção `RabbitMq`), fila `order-placed`, `OrderPlacedConsumer` com `UseTransientRetry` e check do bus em `/health/bus`; teste do consumidor no test harness; verificar com `dotnet test src/backend/Customers/Customers.Tests`

## 6. Infraestrutura e identidade

- [x] 6.1 `customers-db` no `docker-compose.database.yml` (porta 5437, volume `customers_data`) e `customers-api` no `docker-compose.yml` (porta 5230, `Keycloak__MetadataAddress`, `RabbitMq__Host`); verificar com `docker compose up -d --build customers-api` e `curl localhost:5230/health/ready`
- [x] 6.2 Mapper de audience `customers-api-audience` no client `scorponok-web` do `keycloak/scorponok-realm.json`; verificar, após reimportar o realm, que o access token traz `aud` com `orders-api` e `customers-api`

## 7. Front-end

- [x] 7.1 `src/lib/customers.ts` (`VITE_CUSTOMERS_API_URL`, padrão `http://localhost:5230`, no `vite-env.d.ts` e como build arg do `docker-compose.frontend.yml`) com `getProfile` e `saveProfile` usando o access token
- [x] 7.2 `RequireAuth` com texto parametrizável e `ProfilePage` em `/account/profile`: formulário, máscaras, erros por campo, sucesso e falha mantendo os dados
- [x] 7.3 Menu da conta no `Header` (desktop e mobile) com "Meu cadastro" e "Sair"
- [x] 7.4 Checkout com nome, telefone e CPF obrigatórios, enviados em `customer`, e `ShippingAddressForm` no formato do cadastro (número, complemento, bairro, UF em lista, país fixo), com máscaras, validação do cadastro e erros da API exibidos
- [x] 7.5 Checkout pré-preenchido com os dados e o endereço do cadastro (campo a campo) só nos campos vazios, ignorando falha na busca; verificar com `npm run build && npm run lint` em `src/frontend`

## 8. Documentação e verificação

- [x] 8.1 Atualizar o `CLAUDE.md` (comandos, arquitetura do Customers, `address` e `customer` no `POST /createOrder`, recriação do `orders_data`, `OrderPlaced` e fila `order-placed`, compose, realm) e o `src/frontend/CLAUDE.md` (página de cadastro e checkout)
- [x] 8.2 Verificação no compose: cadastro vazio no primeiro acesso, CPF inválido recusado, cadastro com campo obrigatório faltando recusado, cadastro completo salvo e exibido de novo, primeira compra sem cadastro criando o cadastro, checkout aberto com os dados salvos, pedido com outro endereço gravado no histórico sem mudar o cadastro, pedido registrado com o `customers-api` parado e histórico gravado quando ele volta, checkout em branco com o `customers-api` parado
- [x] 8.3 Acrescentar ao `docs/testes/roteiro.md` os cenários do cadastro, do checkout pré-preenchido, da primeira compra e do histórico de endereços

## Workflow follow-up

- Arquivar a change depois da revisão.
