namespace Shared.Code.Commands;

public abstract record Message : IRequest
{
    public string MessageType { get; protected init; }
    public Guid AggregateId { get; protected init; } = Guid.NewGuid();
    protected Message() => MessageType = GetType().Name;
}


