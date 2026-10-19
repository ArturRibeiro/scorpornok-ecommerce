namespace Customers.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    // Commands e queries do cadastro: endpoint ou consumidor → IMemoryBus → MediatR → handler.
    private static IServiceCollection AddCustomerHandlers(this IServiceCollection services)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddScoped<IMemoryBus, MemoryBus>();
        services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        services.AddScoped<IRequestHandler<GetCustomerProfileQuery, CustomerProfile>, CustomerProfileHandlers>();
        services.AddScoped<IRequestHandler<UpdateCustomerProfileCommand, UpdateCustomerProfileResult>, CustomerProfileHandlers>();
        services.AddScoped<IRequestHandler<RecordPurchaseCommand>, RecordPurchaseHandler>();
        return services;
    }
}
