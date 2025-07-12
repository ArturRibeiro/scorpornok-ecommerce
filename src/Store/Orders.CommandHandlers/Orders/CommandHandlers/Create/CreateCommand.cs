namespace Orders.CommandHandlers.Orders.CommandHandlers.Create;

public record CreateCommand(Guid UserId, OrderAddressMessageResponse Address, IList<OrderItemMessageResponse> Items)
    : Message;
