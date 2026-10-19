using System;
using Catalog.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Banco do Catalog (catalog-db); as consultas usam o contexto pela interface IApplicationCatalogDbContext.
    private static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ApplicationCatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped(repoFactory);
        return services;
    }

    private static Func<IServiceProvider, IApplicationCatalogDbContext> repoFactory = x
        => x.GetService<ApplicationCatalogDbContext>();
}
