namespace Customers.Infrastructure.Extensions;

// Registro da infraestrutura do Customers, um arquivo por contexto (ServiceCollectionExtensions.<Contexto>.cs).
public static partial class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString,
        IConfiguration configuration)
    {
        services
            .AddCustomerHandlers()
            .AddPersistence(connectionString)
            .AddMessaging(configuration);
    }
}
