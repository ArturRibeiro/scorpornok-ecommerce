namespace Orders.Infrastructure.Messaging;

public class MassTransitIntegrationEventPublisher(IPublishEndpoint publishEndpoint) : IIntegrationEventPublisher
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => publishEndpoint.Publish(message, cancellationToken);
}
