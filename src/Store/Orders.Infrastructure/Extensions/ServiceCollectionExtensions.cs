using Orders.Infrastructure.Gateways;

namespace Orders.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddScoped<IMemoryBus, MemoryBus>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentGateway, PaymentGateway>();
        services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        services.AddScoped<IRequestHandler<CreateCommand>, OrderHandler>();
        // Npgsql 6+ só aceita DateTime UTC em "timestamp with time zone"; mantém o comportamento
        // anterior porque Order.OrderDate usa DateTime.Now. Remover ao migrar o domínio para UTC.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<OrderContext>(x => x.UseNpgsql(connectionString));
    }
}