namespace Frameworker.Integration.Tests.WebApplicationFactorys.Oracle
{
    public class WebApplicationFactoryOracle<TStartup, TApplicationDbContext> : BaseWebApplicationFactory<TStartup>
        where TStartup : class
        where TApplicationDbContext : DbContext
    {
        public WebApplicationFactoryOracle()
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

                services.AddDbContext<TApplicationDbContext>(builder => throw new NotImplementedException("Provider oracle not implementation."));
            });
        }
    }
}