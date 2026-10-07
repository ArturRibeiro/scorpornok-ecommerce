# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão geral

Scorponok é um backend de e-commerce modular em .NET, feito para estudo e demonstração (DDD, CQRS, MediatR, domain notifications, event sourcing). O README ainda descreve o .NET Core 3.1. O código já migrou para **net8.0** (`Programming.Functional*` ainda é net6.0). Comentários no código, o `use-case.md` e parte da documentação estão em português.

## Comandos

```bash
dotnet build Scorponok.sln                     # compila tudo (CI: restore → build → test no master)
dotnet test Scorponok.sln                      # roda todos os testes
dotnet test src/Frameworker/Programming.Functional.Tests   # um projeto de teste
dotnet test src/Frameworker/Programming.Functional.Tests --filter "FullyQualifiedName~OptionTests"  # uma classe/teste
dotnet run --project src/Catalog/Catalog.Web.Api           # sobe a API de Catalog (precisa de Docker)
dotnet run --project src/Store/Orders.Web.Api              # sobe a API de Orders (precisa de Docker)
docker compose up -d                           # Postgres 16 compartilhado (devuser/devpassword); as APIs ainda não o usam
```

- `Programming.Functional.Tests` é net6.0, então rodá-lo exige o runtime do .NET 6 (ou trocar o target).
- Cada projeto `*.Web.Api` tem um arquivo `.http` para testar os endpoints.

## Versões de pacotes

Todas as versões de pacotes são propriedades MSBuild no `Directory.Build.props`, por exemplo `Version="$(MediatRNet8)"`. Adicione ou atualize versões lá, não direto no csproj. Os projetos usam `<TargetFramework>$(TargetFrameworkNet8)</TargetFramework>`. O arquivo de props define versões legadas (EF Core 3.1.x, MediatR 7) e versões net8 (`*Net8`), e os projetos misturam as duas. Por exemplo, projetos net8 ainda referenciam o EF Core `$(EntityFrameworkCore)` = 3.1.19. Os projetos Web API fixam algumas versões direto no csproj (Swashbuckle, OpenApi, Testcontainers.PostgreSql).

## Arquitetura

Os bounded contexts ficam em `src/`. Cada um é dividido em projetos por camada, e as dependências apontam para dentro, até o Domain:

- **Catalog** (lado de leitura / queries): `Catalog.Domain` (entidade `Product`) ← `Catalog.Queries` (`IProductQueries`, `IApplicationCatalogDbContext`, paginação via `ToPagedList`/`IPagedList` do `Frameworker.EntityFrameworkCore`) ← `Catalog.Infrastructure` (`ApplicationCatalogDbContext`, configurações do EF, `AddInfrastructure`) ← `Catalog.Web.Api`.
- **Store / Orders** (lado de escrita / commands): `Orders.Domain` (agregado `Order`, validadores FluentValidation, `IOrderRepository`) ← `Orders.CommandHandlers` (`IRequestHandler`s do MediatR, como o `OrderHandler`, que monta um `Order` com o `OrderBuilder` fluente; também define `IPaymentGateway`) ← `Orders.Infrastructure` (`OrderContext`, repositório, um `PaymentGateway` stub, DI em `AddInfrastructure`) ← `Orders.Web.Api`.
- **Payments**: `Gateway.Payment.Data` contém o store de event sourcing (`EventStoreContext`, `SqlEventStore`, `StoredEvent`). `Gateway.Payment.Web.Api` ainda é o template de previsão do tempo.
- **Shared/Shared.Code**: os blocos básicos de DDD/CQRS. São eles `Entity<T>` (acumula erros de validação), `ValueObject`, `IAggregateRoot`, `IRepository`/`IUnitOfWork`, `Message`/`CommandBase`/`Event`, a base `CommandHandler` e `DomainNotification` com seu handler. Também tem `IMemoryBus`/`MemoryBus`, um wrapper fino sobre o `IMediator`. O `RaiseEvent` está comentado (não faz nada).
- **Frameworker**: infraestrutura reutilizável. `Programming.Functional` fornece `Result`/`Result<T>`/`Maybe<T>`/`Option<T>`. `Frameworker.EntityFrameworkCore` fornece paginação. `Frameworker.Integration.Tests` fornece bases de `WebApplicationFactory` com Testcontainers (Postgres/Oracle/Sqlite), que escolhem o `appsettings.<dbtype>.json`.

### Convenções das Web APIs

- Minimal APIs: os endpoints são métodos de extensão de `WebApplication` em `WebApplicationExtensions/` (ex.: `app.GetAllProducts()`, `app.CreateOrder()`), chamados no `Program.cs`.
- Os commands chegam aos handlers assim: endpoint → `IMemoryBus.SendAsync(command)` → MediatR → handler. Os handlers são registrados explicitamente em cada `Infrastructure/Extensions/ServiceCollectionExtensions.cs`, junto com o scan do assembly.
- **Na inicialização, cada API sobe seu próprio container Postgres temporário via Testcontainers** (`DockerPostgreSql.cs`). Depois chama `EnsureCreatedAsync()` e popula dados (`*DbContextExtensions.Seed`). Não há migrations do EF. Rodar uma API exige o daemon do Docker ativo.
- `Program` é declarado como `public partial class Program` para que os testes possam usar `WebApplicationFactory<Program>`.

## Testes

- `src/Tests/Ecommerce.Scenarios.Integration.Spec.Tests`: testes BDD com SpecFlow + NUnit contra o `Catalog.Web.Api` via `WebApplicationFactory<Program>` (`Hooks/Hook.cs`). O `Features/*.feature.cs` é gerado no build, então edite o arquivo `.feature`.
- `src/Frameworker/Programming.Functional.Tests`: testes unitários com NUnit + FluentAssertions + Moq.
- `src/Tests/Ecommerce.Integration.Tests` **não está na solution** e está desatualizado. É netcoreapp3.1 e referencia projetos que não existem mais (`Store.Web.Api`, `Frameworker.Scorponok.*`).
- `Snippets/` contém snippets do Visual Studio com o layout dos testes unitários.
