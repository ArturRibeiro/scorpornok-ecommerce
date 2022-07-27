// using Catalog.Infrastructure;
// using Ecommerce.Scenarios.Integration.Tests.Seed.Catalogs;
// using StartupCatalog = Catalog.Web.Api.Startup;
// using NUnit.Framework;
// using Frameworker.Integration.Tests.WebApplicationFactorys.Extensions;
// using Frameworker.Integration.Tests.WebApplicationFactorys.Postgres;
//
// namespace Ecommerce.Scenarios.Integration.Tests
// {
//     [SetUpFixture]
//     public class Global
//     {
//         private readonly WebApplicationFactoryPostgres<StartupCatalog, ApplicationCatalogDbContext> _factory;
//
//         public Global() => _factory = new WebApplicationFactoryPostgres<StartupCatalog, ApplicationCatalogDbContext>();
//
//         [OneTimeSetUp]
//         public void Setup()
//         {
//             _factory.InitializeContainer()
//                 .Server.Host
//                 .ErasureDatabase<ApplicationCatalogDbContext>()
//                 .CreateDataBase<ApplicationCatalogDbContext>((context, services) =>
//                 {
//                     context.Products.AddRange(Products.CreateProducts(20));
//                     context.SaveChanges();
//                 });
//         }
//
//         [OneTimeTearDown]
//         public void Cleanup() => _factory.FinalizeContainer();
//     }
// }