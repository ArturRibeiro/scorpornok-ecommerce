using MassTransit;

namespace Payments.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Consome a solicitação e a consulta do pagamento e publica o resultado (RabbitMq no appsettings).
    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<PaymentRequestedConsumer>();
            x.AddConsumer<PaymentStatusRequestedConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMq = configuration.GetSection("RabbitMq");
                cfg.Host(rabbitMq["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rabbitMq["Username"] ?? "guest");
                    h.Password(rabbitMq["Password"] ?? "guest");
                });
                cfg.UseTransientRetry();
                cfg.ConfigureEndpoints(context);
            });
        });
        return services;
    }
}
