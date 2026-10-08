namespace Shared.Code.Commands;

public abstract record Message : IRequest
{
    public string MessageType { get; protected init; }
    public Guid AggregateId { get; protected init; } = Guid.NewGuid();
    protected Message() => MessageType = GetType().Name;
}

/// <summary>
/// Mensagem cujo handler devolve uma resposta (ex.: o resultado de criar um pedido).
/// Enviada pelo <see cref="IMemoryBus.RequestAsync{TResponse}"/>.
/// </summary>
public abstract record Message<TResponse> : Message, IRequest<TResponse>;
