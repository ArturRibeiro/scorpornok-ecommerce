namespace Orders.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Publica PaymentRequested e OrderPlaced pelo outbox e consome o resultado do pagamento
    // (RabbitMq no appsettings).
    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<PaymentApprovedConsumer>();
            x.AddConsumer<PaymentRejectedConsumer>();
            x.AddConsumer<PaymentNotProcessedConsumer>();
            x.AddConsumer<PaymentStatusPendingConsumer>();
            // Bus outbox: o IPublishEndpoint da requisição grava a mensagem no OrderContext
            // (no SaveChanges) e um serviço em segundo plano a entrega quando o broker responde.
            x.AddEntityFrameworkOutbox<OrderContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
            // O check do bus vem com a tag "ready"; com o outbox, o pedido não depende do broker,
            // então ele sai do /health/ready e fica só no /health/bus.
            x.ConfigureHealthCheckOptions(o =>
            {
                o.Tags.Clear();
                o.Tags.Add("bus");
            });
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
