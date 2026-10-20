namespace Catalog.Infrastructure.Extensions;

// Registro da infraestrutura do Catalog, um arquivo por contexto (ServiceCollectionExtensions.<Contexto>.cs).
public static partial class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString)
        => services
            .AddProductQueries()
            .AddPersistence(connectionString);

    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services.AddInfrastructure(configuration.GetConnectionString("ConnectionString"));
}
