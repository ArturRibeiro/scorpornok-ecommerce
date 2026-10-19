namespace Payments.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Commands do pagamento e do token do cartão: endpoint ou consumidor → IMemoryBus → MediatR → handler.
    private static IServiceCollection AddPaymentHandlers(this IServiceCollection services)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(ProcessPaymentHandler).Assembly));
        services.AddScoped<IMemoryBus, MemoryBus>();
        services.AddScoped<IRequestHandler<ProcessPaymentCommand>, ProcessPaymentHandler>();
        services.AddScoped<IRequestHandler<CheckPaymentStatusCommand>, CheckPaymentStatusHandler>();
        services.AddScoped<IRequestHandler<CreateCardTokenCommand, CreateCardTokenResult>, CreateCardTokenHandler>();
        return services;
    }
}
