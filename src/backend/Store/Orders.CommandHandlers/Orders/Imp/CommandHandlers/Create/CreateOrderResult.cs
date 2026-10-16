namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;

/// <summary>Desfecho do registro: criado, recusado pela validação ou impossível agora (Catalog fora).</summary>
public enum CreateOrderOutcome
{
    Created,
    Invalid,
    Unavailable
}

/// <summary>
/// Resultado de <see cref="CreateCommand"/>: os dados do pedido registrado, as mensagens de validação
/// ou a indisponibilidade de uma dependência.
/// </summary>
public record CreateOrderResult(
    CreateOrderOutcome Outcome,
    string? OrderNumber,
    string? Status,
    decimal Total,
    IReadOnlyCollection<string> Errors)
{
    public static CreateOrderResult Created(Order order)
        => new(CreateOrderOutcome.Created, order.OrderNumber, order.Status.Name, order.Total, []);

    public static CreateOrderResult Invalid(IReadOnlyCollection<string> errors)
        => new(CreateOrderOutcome.Invalid, null, null, 0, errors);

    public static CreateOrderResult Unavailable(string reason)
        => new(CreateOrderOutcome.Unavailable, null, null, 0, [reason]);
}
