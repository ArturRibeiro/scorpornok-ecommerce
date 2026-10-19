namespace Customers.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Consome o OrderPlaced do Orders (fila order-placed; seção RabbitMq no appsettings).
    private static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<OrderPlacedConsumer>();
            // O check do bus vem com a tag "ready"; o cadastro não depende do broker, então ele
            // sai do /health/ready e fica só no /health/bus, como no Orders.
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
