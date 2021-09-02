using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Catalog.Web.Api;
using Gateway.Payment.Web.Api;
using Store.Web.Api;

namespace Ecommerce.Integration.Tests
{
    public static class WebHostBuilderHelper
    {
        public static TestServer CreateInMemoryTestServer<TStartup>(string path) where TStartup : class
        {
            var projectName = typeof(TStartup).Assembly.GetName().Name;
            var pathWebApi = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", path, projectName));
            var webHostBuilder = CreateTestServer(pathWebApi)
                .UseStartup<TStartup>();
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
            this._serverPayment = WebHostBuilderHelper.CreateInMemoryTestServer<StartupPayment>("Payments");
            this._serverOrder = WebHostBuilderHelper.CreateInMemoryTestServer<StartupStore>("Store");
            this._serverCatalog = WebHostBuilderHelper.CreateInMemoryTestServer<StartupCatalog>("Catalog");

            return this;
        }
    }
}
