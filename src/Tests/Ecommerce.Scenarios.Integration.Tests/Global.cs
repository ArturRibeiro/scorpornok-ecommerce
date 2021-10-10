using Catalog.Infrastructure;
using Ecommerce.Scenarios.Integration.Tests.Seed.Catalogs;
using StartupCatalog = Catalog.Web.Api.Startup;
using NUnit.Framework;
using Frameworker.Integration.Tests.Sqlite;
using Frameworker.Integration.Tests.WebHostExtensions;

namespace Ecommerce.Scenarios.Integration.Tests
{
    [SetUpFixture]
    public class Global
    {
        [OneTimeSetUp]
        public void Setup()
        {
            var factory = new WebApplicationFactoryWithSqlite<StartupCatalog, ApplicationCatalogDbContext>()
                .Server
                .Host
                .ErasureDatabase<ApplicationCatalogDbContext>()
                .CreateDataBase<ApplicationCatalogDbContext>((context, services) =>
                {
                    context.Products.AddRange(Products.GetAll(20));
                    context.SaveChanges();
                });
        }
        
        
    }
}