using MassTransit;

namespace Payments.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString,
        IConfiguration configuration)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(ProcessPaymentHandler).Assembly));
        services.AddScoped<IMemoryBus, MemoryBus>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICardTokenRepository, CardTokenRepository>();
        // Singleton: o gateway simulado guarda o resultado por chave de idempotência.
        services.AddSingleton<IPaymentGateway, PaymentGateway>();
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        services.AddScoped<IRequestHandler<ProcessPaymentCommand>, ProcessPaymentHandler>();
        services.AddScoped<IRequestHandler<CheckPaymentStatusCommand>, CheckPaymentStatusHandler>();
        services.AddScoped<IRequestHandler<CreateCardTokenCommand, CreateCardTokenResult>, CreateCardTokenHandler>();
        services.AddDbContext<PaymentContext>(x => x.UseNpgsql(connectionString));
        // /health/ops: mensagens paradas nas filas de erro dos consumidores do Payments.
        var queues = KebabCaseEndpointNameFormatter.Instance;
        services.AddErrorQueuesHealthCheck(configuration,
            queues.Consumer<PaymentRequestedConsumer>(), queues.Consumer<PaymentStatusRequestedConsumer>());

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
    }
}
