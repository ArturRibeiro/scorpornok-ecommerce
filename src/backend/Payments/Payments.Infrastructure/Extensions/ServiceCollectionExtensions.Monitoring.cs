using MassTransit;

namespace Payments.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // /health/ops: mensagens paradas nas filas de erro dos consumidores do Payments.
    private static IServiceCollection AddMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        var queues = KebabCaseEndpointNameFormatter.Instance;
        services.AddErrorQueuesHealthCheck(configuration,
            queues.Consumer<PaymentRequestedConsumer>(), queues.Consumer<PaymentStatusRequestedConsumer>());
        return services;
    }
}
