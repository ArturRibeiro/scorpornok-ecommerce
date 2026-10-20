namespace Frameworker.Integration.Tests.WebApplicationFactorys.Postgres
{
    public class WebApplicationFactoryPostgres<TStartup, TApplicationDbContext> : BaseWebApplicationFactory<TStartup>
        where TStartup : class
        where TApplicationDbContext : DbContext
    {
        public WebApplicationFactoryPostgres()
            : base(DataBaseType.Postgres)
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll(typeof(TApplicationDbContext));
                services
                    .AddEntityFrameworkNpgsql()
                    .AddDbContext<TApplicationDbContext>(options => { options.UseNpgsql(Container.ConnectionString); });
            });
        }
    }
}