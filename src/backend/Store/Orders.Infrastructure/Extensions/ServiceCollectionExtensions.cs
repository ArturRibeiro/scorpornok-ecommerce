namespace Orders.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, string connectionString,
        IConfiguration configuration)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddScoped<IMemoryBus, MemoryBus>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        services.AddScoped<IRequestHandler<CreateCommand, CreateOrderResult>, OrderHandler>();
        services.AddScoped<IRequestHandler<ConfirmOrderPaymentCommand>, OrderPaymentHandlers>();
        services.AddScoped<IRequestHandler<FailOrderPaymentCommand>, OrderPaymentHandlers>();
        services.AddScoped<IRequestHandler<GetOrderStatusQuery, string?>, OrderPaymentHandlers>();
        services.AddScoped<IIntegrationEventPublisher, MassTransitIntegrationEventPublisher>();
        // E-mail com o resultado do pagamento (seção Smtp; Mailpit no compose).
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.Section));
        services.AddScoped<IOrderEmailSender, SmtpOrderEmailSender>();
        services.AddHostedService<OrderEmailDispatcher>();
        // Npgsql 6+ só aceita DateTime UTC em "timestamp with time zone"; mantém o comportamento
        // anterior porque Order.OrderDate usa DateTime.Now. Remover ao migrar o domínio para UTC.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        services.AddDbContext<OrderContext>(x => x.UseNpgsql(connectionString));

        // Publica PaymentRequested e consome o resultado do pagamento (RabbitMq no appsettings).
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumer<PaymentApprovedConsumer>();
            x.AddConsumer<PaymentRejectedConsumer>();
            // Bus outbox: o IPublishEndpoint da requisição grava a mensagem no OrderContext
            // (no SaveChanges) e um serviço em segundo plano a entrega quando o broker responde.
            x.AddEntityFrameworkOutbox<OrderContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });
            // O check do bus vem com a tag "ready"; com o outbox, o pedido não depende do broker,
            // então ele sai do /health/ready e fica só no /health/bus.
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
                cfg.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(1)));
                cfg.ConfigureEndpoints(context);
            });
        });
    }
}