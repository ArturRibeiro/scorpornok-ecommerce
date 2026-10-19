namespace Payments.Infrastructure.Extensions;

// Registro da infraestrutura do Payments, um arquivo por contexto (ServiceCollectionExtensions.<Contexto>.cs).
public static partial class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString,
        IConfiguration configuration)
    {
        services
            .AddPaymentHandlers()
            .AddPersistence(connectionString)
            .AddPaymentGateway()
            .AddMonitoring(configuration)
            .AddMessaging(configuration);
    }
}
