namespace Orders.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Pedido pendente além do prazo: consulta o Payments (seção PaymentCheck).
    private static IServiceCollection AddPaymentCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaymentCheckOptions>(configuration.GetSection(PaymentCheckOptions.Section));
        services.AddHostedService<PendingOrderWatcher>();
        return services;
    }
}
