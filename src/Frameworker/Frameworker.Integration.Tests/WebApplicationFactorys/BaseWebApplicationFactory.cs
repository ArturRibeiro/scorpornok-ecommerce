using System;
using System.IO;
using System.Net.Http;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Frameworker.Integration.Tests.WebApplicationFactorys
{
    public abstract class BaseWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private readonly string _currentDirectory = Directory.GetCurrentDirectory();
        private readonly string _appsettings;
        private bool appsettingsExist => File.Exists(@$"{_currentDirectory}//{_appsettings}");

        protected TestcontainerDatabase Container;

        protected BaseWebApplicationFactory(DataBaseType dataBaseType)
        {
            Container = ContainerDataBaseFactory.Create(dataBaseType);
            _appsettings = string.Format("appsettings.{0}.json", dataBaseType.ToString().ToLower());
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    if (!appsettingsExist) throw new FileNotFoundException($"{_currentDirectory}/{_appsettings}");
                    config
                        .SetBasePath(_currentDirectory)
                        .AddJsonFile(_appsettings);
                })
                .UseStartup<TStartup>();
        }

        protected override void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri("https://localhost:5001");
            
            base.ConfigureClient(client);
        }
        
        public BaseWebApplicationFactory<TStartup> InitializeContainer()
        {
            Container.StartAsync().GetAwaiter().GetResult();
            return this;
        }

        public BaseWebApplicationFactory<TStartup> FinalizeContainer()
        {
            Container.DisposeAsync().GetAwaiter().GetResult();
            return this;
        }

        //public string GetConnectionString() => this.Container.ConnectionString;
    }
}