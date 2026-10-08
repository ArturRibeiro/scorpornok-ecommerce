namespace Shared.Code.IntegrationEvents;

/// <summary>
/// Publica mensagens de integração entre contextos. Os handlers dependem só desta
/// interface; cada Infrastructure a implementa sobre o broker (MassTransit).
/// </summary>
public interface IIntegrationEventPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
