using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Frameworker.Integration.Tests.Sqlite
{
    public class WebApplicationFactoryWithInMemorySqlite<TStartup, TApplicationDbContext> : BaseWebApplicationFactory<TStartup>
        where TStartup : class
        where TApplicationDbContext : DbContext
    {
        private readonly string _connectionString = "DataSource=:memory:";
        private readonly SqliteConnection _connection;

        public WebApplicationFactoryWithInMemorySqlite()
        {
            _connection = new SqliteConnection(_connectionString);
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureServices(services =>
            {
                services
                    .AddEntityFrameworkSqlite()
                    .AddDbContext<TApplicationDbContext>(options =>
                    {
                        options.UseSqlite(_connection);
                        options.UseInternalServiceProvider(services.BuildServiceProvider());
                    });
            });

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection.Close();
        }
    }
}