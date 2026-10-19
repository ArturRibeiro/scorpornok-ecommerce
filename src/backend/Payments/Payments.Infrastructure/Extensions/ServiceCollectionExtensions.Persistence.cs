namespace Payments.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Banco do Payments (payment-db): pagamentos e tokens do cartão.
    private static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaymentContext>(x => x.UseNpgsql(connectionString));
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ICardTokenRepository, CardTokenRepository>();
        return services;
    }
}
