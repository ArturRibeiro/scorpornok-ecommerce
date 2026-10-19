namespace Orders.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // /health/ops: o que a operação precisa acompanhar, fora do ready.
    private static IServiceCollection AddMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        // Mensagens paradas nas filas de erro dos consumidores do Orders.
        var queues = KebabCaseEndpointNameFormatter.Instance;
        services.AddErrorQueuesHealthCheck(configuration,
            queues.Consumer<PaymentApprovedConsumer>(), queues.Consumer<PaymentRejectedConsumer>(),
            queues.Consumer<PaymentNotProcessedConsumer>(), queues.Consumer<PaymentStatusPendingConsumer>());
        // Pedidos pendentes além do prazo e da tolerância.
        services.AddHealthChecks().AddCheck<StuckOrdersHealthCheck>(StuckOrdersHealthCheck.Name, tags: ["ops"]);
        return services;
    }
}
