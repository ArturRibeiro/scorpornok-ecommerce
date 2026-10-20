namespace Catalog.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Consultas de produtos (lado de leitura).
    private static IServiceCollection AddProductQueries(this IServiceCollection services)
    {
        services.AddScoped<IProductQueries, ProductQueries>();
        return services;
    }
}
