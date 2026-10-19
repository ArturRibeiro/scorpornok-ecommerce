namespace Orders.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Commands e queries do pedido: endpoint ou consumidor → IMemoryBus → MediatR → handler.
    private static IServiceCollection AddOrderHandlers(this IServiceCollection services)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddScoped<IMemoryBus, MemoryBus>();
        services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        services.AddScoped<IRequestHandler<CreateCommand, CreateOrderResult>, OrderHandler>();
        services.AddScoped<IRequestHandler<ConfirmOrderPaymentCommand>, OrderPaymentHandlers>();
        services.AddScoped<IRequestHandler<FailOrderPaymentCommand>, OrderPaymentHandlers>();
        services.AddScoped<IRequestHandler<MarkOrderPaymentNotProcessedCommand>, OrderPaymentHandlers>();
        services.AddScoped<IRequestHandler<GetOrderStatusQuery, OrderPaymentState?>, OrderPaymentHandlers>();
        services.AddScoped<IRequestHandler<RetryOrderPaymentCommand, RetryOrderPaymentResult>, RetryOrderPaymentHandler>();
        return services;
    }
}
