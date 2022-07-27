using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catalog.Infrastructure;
using Catalog.Web.Api;
using Ecommerce.Scenarios.Integration.Spec.Tests.Seed.Catalogs;
using Frameworker.Integration.Tests.WebApplicationFactorys.Extensions;
using Frameworker.Integration.Tests.WebApplicationFactorys.Postgres;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;

namespace Ecommerce.Scenarios.Integration.Spec.Tests.Hooks
{
    [Binding]
    public sealed class Hook
    {
        private readonly WebApplicationFactoryPostgres<Startup, ApplicationCatalogDbContext> _factory;

        public Hook() => _factory = new WebApplicationFactoryPostgres<Startup, ApplicationCatalogDbContext>();

        [BeforeScenario]
        public void BeforeScenario()
        {

            var webHost = _factory.InitializeContainer().Server.Host;
            webHost.ErasureDatabase<ApplicationCatalogDbContext>()
                .CreateDataBase<ApplicationCatalogDbContext>((context, services) =>
                {
                    context.Products.AddRange(Products.CreateProducts(20));
                    context.SaveChanges();
                });
        }


        [AfterScenario]

            public void AfterScenario()
            {
                _factory.FinalizeContainer();
            }
        }
    }