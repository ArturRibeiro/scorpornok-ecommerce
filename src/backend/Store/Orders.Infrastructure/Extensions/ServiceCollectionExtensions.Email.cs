namespace Orders.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // E-mail com o resultado do pagamento (seção Smtp; Mailpit no compose) e o /health/email.
    private static IServiceCollection AddPaymentResultEmail(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.Section))
            .Validate(o => o.IsValid, "Smtp: PollSeconds, MaxEmailAttempts, RetryBaseSeconds e RetryMaxSeconds devem ser positivos.")
            .ValidateOnStart();
        // Estado do envio (servidor de e-mail fora ou não), lido pelo /health/email.
        services.AddSingleton<EmailDeliveryState>();
        services.AddHealthChecks().AddCheck<EmailDeliveryHealthCheck>(EmailDeliveryHealthCheck.Name, tags: ["email"]);
        services.AddScoped<IOrderEmailSender, SmtpOrderEmailSender>();
        services.AddHostedService<OrderEmailDispatcher>();
        return services;
    }
}
