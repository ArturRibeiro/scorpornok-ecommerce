# A simple, modularized e-commerce system based on .NET


![5](https://user-images.githubusercontent.com/1322130/131916590-daf90d7f-ccb1-4013-9d59-ca970a04f0a1.png)

What is the ecommerce Project?
=====================
The ecommerce Project is a open-source project written in .NET 10

The goal of this project is implement the most common used technologies and share with the technical community the best way to develop great applications with .NET and evoluting learn.

# Prerequisite
- .NET SDK 10.0
- Docker (the databases run with Docker Compose)

# Technologies used
- [x] Entity Framework Core 10 (Npgsql / PostgreSQL)
- [x] MediatR 13
- [x] FluentValidation
- [x] Swagger UI (Swashbuckle)
- [x] ASP.NET Core Minimal APIs
- [x] Testcontainers
- [x] SpecFlow + NUnit (integration scenarios)
- [ ] ASP.NET Core Identity
- [ ] Angular 1.6.3
- [ ] AutoMapper

## Architecture:
- Responsibility separation concerns, SOLID and Clean Code
- Domain Driven Design
- Domain Events
- Domain Notification
- CQRS
- Event Sourcing
- Unit of Work
- Repository and Generic Repository



# How to use
- You will need the .NET SDK 10.0 or higher, available at https://dot.net
- Make sure the Docker daemon is running and the databases are up: `docker compose -f docker-compose.database.yml up -d`

```bash
dotnet build Scorponok.slnx                         # build everything
dotnet test Scorponok.slnx                          # run all tests
dotnet run --project src/backend/Catalog/Catalog.Web.Api    # start the Catalog API
dotnet run --project src/backend/Store/Orders.Web.Api       # start the Orders API
```

Each `*.Web.Api` project has a `.http` file to exercise its endpoints.

# Boas práticas

## Health checks

Cada API tem dois endpoints que informam se ela está funcionando:

- `/health/live`: a API está no ar. Se falhar, reinicie a API.
- `/health/ready`: a API consegue atender pedidos, incluindo o acesso ao banco. Se falhar, espere ela se recuperar.

| API | Endereço | O que o `/health/ready` verifica |
|---|---|---|
| Catalog | http://localhost:5064/health/live e http://localhost:5064/health/ready | Banco de produtos |
| Orders | http://localhost:5224/health/live e http://localhost:5224/health/ready | Banco de pedidos |
| Payment | http://localhost:5220/health/live e http://localhost:5220/health/ready | Nada além da própria API, pois ela não usa banco |

O `/health/live` não verifica o banco. Assim, uma queda do banco não faz as APIs reiniciarem.
