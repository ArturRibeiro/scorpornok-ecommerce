namespace Orders.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Preço, nome e foto dos itens vêm do Catalog (obrigatório, como a connection string).
    private static IServiceCollection AddCatalog(this IServiceCollection services, IConfiguration configuration)
    {
        var catalogAddress = configuration[$"{HttpCatalogProducts.Section}:BaseAddress"]
            ?? throw new InvalidOperationException("Catalog:BaseAddress não configurado.");
        services.AddHttpClient<ICatalogProducts, HttpCatalogProducts>(client =>
        {
            client.BaseAddress = new Uri(catalogAddress.TrimEnd('/') + "/");
            client.Timeout = HttpCatalogProducts.Timeout;
        });
        return services;
    }
}
