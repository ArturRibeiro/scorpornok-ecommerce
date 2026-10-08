namespace Shared.Code.Imp.Bus;

public sealed class MemoryBus : IMemoryBus
{
    private readonly IMediator _mediator;

    public MemoryBus(IMediator mediator) => _mediator = mediator;

    public async Task RaiseEvent<T>(T @event) where T : Event
    {
        // //if (!@event.MessageType.Equals("DomainNotification"))
        // //    _eventStore?.Save(@event);
        // await _mediator.Publish(@event);
    }

    public async Task SendAsync<T>
        (T command) 
        where T : Message
    {
        //var message = command as Message;
        //string serialize = System.Text.Json.JsonSerializer.Serialize(message);
        await _mediator.Send(command);
    }

    // O cast escolhe a sobrecarga Send(IRequest<TResponse>) do MediatR; sem ele,
    // Send<TRequest>(TRequest) casaria primeiro e a resposta se perderia.
    public Task<TResponse> RequestAsync<TResponse>(Message<TResponse> command)
        => _mediator.Send((IRequest<TResponse>)command);
}
