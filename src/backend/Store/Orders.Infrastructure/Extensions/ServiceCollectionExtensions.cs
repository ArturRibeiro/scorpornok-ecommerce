namespace Orders.Infrastructure.Extensions;

// Registro da infraestrutura do Orders, um arquivo por contexto (ServiceCollectionExtensions.<Contexto>.cs).
public static partial class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString,
        IConfiguration configuration)
    {
        services
            .AddOrderHandlers()
            .AddPersistence(connectionString)
            .AddCatalog(configuration)
            .AddPaymentResultEmail(configuration)
            .AddPaymentCheck(configuration)
            .AddMonitoring(configuration)
            .AddMessaging(configuration);
    }
}
