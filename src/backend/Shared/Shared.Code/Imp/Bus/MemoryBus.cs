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
}