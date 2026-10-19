namespace Customers.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Banco do Customers (customers-db): cadastros e histórico de endereços de entrega.
    private static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        // Npgsql 6+ só aceita DateTime UTC em "timestamp with time zone"; a data do pedido vem do
        // Orders, que usa DateTime.Now. Mesmo ajuste do Orders.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<CustomerContext>(x => x.UseNpgsql(connectionString));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        return services;
    }
}
