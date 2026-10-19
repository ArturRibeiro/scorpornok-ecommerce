namespace Orders.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Banco do Orders (orders-db) e o repositório do pedido.
    private static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        // Npgsql 6+ só aceita DateTime UTC em "timestamp with time zone"; mantém o comportamento
        // anterior porque Order.OrderDate usa DateTime.Now. Remover ao migrar o domínio para UTC.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<OrderContext>(x => x.UseNpgsql(connectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }
}
