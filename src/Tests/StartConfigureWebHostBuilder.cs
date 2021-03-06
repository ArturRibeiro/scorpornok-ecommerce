﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Frameworker.Scorponok.Tests.WebHost.Extensions;
using PaymentStartup = Gateway.Payment.Web.Api.Startup;
using StoreStartup = Store.Web.Api.Startup;
using CatalogStartup = Catalog.Web.Api.Startup;

namespace Ecommerce.Integration.Tests
{
    public static class WebHostBuilderHelper
    {
        public static TestServer CreateInMemoryTestServerPayment()
        {
            var projectName = typeof(PaymentStartup).Assembly.GetName().Name;
            var pathWebApi = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Payments", projectName));
            var webHostBuilder = CreateTestServer(pathWebApi)
                .UseStartup<PaymentStartup>();
            return new TestServer(webHostBuilder);
        }

        public static TestServer CreateInMemoryTestServerOrder()
        {
            var projectName = typeof(StoreStartup).Assembly.GetName().Name;
            var pathWebApi = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Store", projectName));
            var webHostBuilder = CreateTestServer(pathWebApi)
                .UseStartup<StoreStartup>();
            return new TestServer(webHostBuilder);
        }

        public static TestServer CreateInMemoryTestServerCatalog()
        {
            var projectName = typeof(CatalogStartup).Assembly.GetName().Name;
            var pathWebApi = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Catalog", projectName));
            var webHostBuilder = CreateTestServer(pathWebApi)
                .UseStartup<CatalogStartup>();
            return new TestServer(webHostBuilder);
        }

        private static IWebHostBuilder CreateTestServer(string pathWebApi)
            => new WebHostBuilder()
                .UseEnvironment("Development")
                .UseContentRoot(pathWebApi)
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(pathWebApi)
                    .Build());
    }

    public partial class Start
    {
        public Start ConfigureWebHostBuilder()
        {
            this._serverPayment = WebHostBuilderHelper.CreateInMemoryTestServerPayment();
            this._serverOrder = WebHostBuilderHelper.CreateInMemoryTestServerOrder();
            this._serverCatalog = WebHostBuilderHelper.CreateInMemoryTestServerCatalog();

            return this;
        }
    }
}
