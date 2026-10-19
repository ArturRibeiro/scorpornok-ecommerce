namespace Payments.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Singleton: o gateway simulado guarda o resultado por chave de idempotência.
    private static IServiceCollection AddPaymentGateway(this IServiceCollection services)
    {
        services.AddSingleton<IPaymentGateway, PaymentGateway>();
        return services;
    }
}
