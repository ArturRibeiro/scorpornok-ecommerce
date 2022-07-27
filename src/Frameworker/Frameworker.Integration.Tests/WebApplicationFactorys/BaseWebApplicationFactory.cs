using System;
using System.IO;
using System.Net.Http;
using DotNet.Testcontainers.Containers;
using Frameworker.Integration.Tests.WebApplicationFactorys.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Frameworker.Integration.Tests.WebApplicationFactorys
{
    public abstract class BaseWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private readonly string _currentDirectory = Directory.GetCurrentDirectory();
        private readonly string _appsettings;
        protected TestcontainerDatabase Container;

        #region Properties
        
        /// <summary>
        /// Represents the settings for the tests
        /// </summary>
        private ApplicationConfiguration ApplicationConfiguration { get; set; }

        #endregion

        protected BaseWebApplicationFactory(DataBaseType dataBaseType)
        {
            Container = ContainerDataBaseFactory.Create(dataBaseType);
            _appsettings = $"appsettings.{dataBaseType.ToString().ToLower()}.json";
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.ValidTheExistenceOfAppsseting($"{_currentDirectory}/{_appsettings}")
                        .SetBasePath(_currentDirectory)
                        .AddJsonFile(_appsettings);
                        
                    var configurationRoot = builder.Build();
                    this.ApplicationConfiguration = configurationRoot.AddConfigurationToTheTest<ApplicationConfiguration>();
                })
                .UseStartup<TStartup>();
        }


        protected override void ConfigureClient(HttpClient client)
        {
            this.Services.GetService<IConfiguration>();
            client.BaseAddress = new Uri(this.ApplicationConfiguration.Uri);
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
    }
}