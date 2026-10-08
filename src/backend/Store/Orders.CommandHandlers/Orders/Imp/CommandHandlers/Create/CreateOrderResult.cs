namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;

/// <summary>
/// Resultado de <see cref="CreateCommand"/>: os dados do pedido registrado ou as mensagens de validação.
/// </summary>
public record CreateOrderResult(
    bool Success,
    string? OrderNumber,
    string? Status,
    decimal Total,
    IReadOnlyCollection<string> Errors)
{
    public static CreateOrderResult Created(Order order)
        => new(true, order.OrderNumber, order.Status.Name, order.Total, []);

    public static CreateOrderResult Invalid(IReadOnlyCollection<string> errors)
        => new(false, null, null, 0, errors);
}
