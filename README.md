# A simple, modularized e-commerce system based on .NET


![5](https://user-images.githubusercontent.com/1322130/131916590-daf90d7f-ccb1-4013-9d59-ca970a04f0a1.png)

What is the ecommerce Project?
=====================
The ecommerce Project is a open-source project written in .NET 10

The goal of this project is implement the most common used technologies and share with the technical community the best way to develop great applications with .NET and evoluting learn.

# Prerequisite
- .NET SDK 10.0 (version pinned in `global.json`)
- Docker (the Web APIs and the integration tests start a temporary PostgreSQL container via Testcontainers)

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
- Make sure the Docker daemon is running

```bash
dotnet build Scorponok.sln                          # build everything
dotnet test Scorponok.sln                           # run all tests
dotnet run --project src/Catalog/Catalog.Web.Api    # start the Catalog API
dotnet run --project src/Store/Orders.Web.Api       # start the Orders API
```

Each `*.Web.Api` project has a `.http` file to exercise its endpoints.
