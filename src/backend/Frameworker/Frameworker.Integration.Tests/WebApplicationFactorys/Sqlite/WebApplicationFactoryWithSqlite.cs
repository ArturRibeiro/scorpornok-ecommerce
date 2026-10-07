using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Frameworker.Integration.Tests.WebApplicationFactorys.Sqlite
{
    /// <summary>
    /// Additional services for the host the web application
    /// </summary>
    /// <typeparam name="TStartup">Startup application</typeparam>
    /// <typeparam name="TApplicationDbContext">DbContext application</typeparam>
    public class WebApplicationFactoryWithSqlite<TStartup, TApplicationDbContext> : BaseWebApplicationFactory<TStartup>
        where TStartup : class
        where TApplicationDbContext : DbContext
    {
        public WebApplicationFactoryWithSqlite()
            : base(DataBaseType.Sqlite)
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureServices(services =>
            {
                services
                    .AddEntityFrameworkSqlite()
                    .AddDbContext<TApplicationDbContext>(options =>
                    {
                        var provider = services.BuildServiceProvider();
                        var configuration = provider.GetService<IConfiguration>();
                        var value = configuration.GetConnectionString("DefaultConnection");
                        //var connectionString = string.Format(value, Directory.GetCurrentDirectory());

                        options.UseSqlite(value);
                        options.UseInternalServiceProvider(provider);
                    });
            });
    }
}