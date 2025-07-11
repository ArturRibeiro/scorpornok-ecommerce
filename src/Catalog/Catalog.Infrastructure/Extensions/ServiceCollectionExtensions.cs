using System;
using Catalog.Queries;
using Catalog.Queries.Products.Queries;
using Catalog.Queries.Products.Queries.Impl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{

    public static void AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<IProductQueries, ProductQueries>();
        services
            .AddDbContext<ApplicationCatalogDbContext>(options =>                    
                options.UseNpgsql(connectionString));

        services.AddScoped(repoFactory);
    }

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProductQueries, ProductQueries>();
        services
            .AddDbContext<ApplicationCatalogDbContext>(options =>                    
                options.UseNpgsql(configuration.GetConnectionString("ConnectionString")));

        services.AddScoped(repoFactory);
    }
    
    private static Func<IServiceProvider, IApplicationCatalogDbContext> repoFactory = x 
        => x.GetService<ApplicationCatalogDbContext>();
}