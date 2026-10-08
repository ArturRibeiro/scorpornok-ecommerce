using MassTransit;

namespace Gateway.Payment.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString,
        IConfiguration configuration)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(ProcessPaymentHandler).Assembly));
        services.AddScoped<IMemoryBus, MemoryBus>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPaymentGateway, PaymentGateway>();
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        services.AddScoped<IRequestHandler<ProcessPaymentCommand>, ProcessPaymentHandler>();
        services.AddDbContext<PaymentContext>(x => x.UseNpgsql(connectionString));

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<PaymentRequestedConsumer>();
            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMq = configuration.GetSection("RabbitMq");
                cfg.Host(rabbitMq["Host"] ?? "localhost", "/", h =>
                {
                    h.Username(rabbitMq["Username"] ?? "guest");
                    h.Password(rabbitMq["Password"] ?? "guest");
                });
                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)));
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}
