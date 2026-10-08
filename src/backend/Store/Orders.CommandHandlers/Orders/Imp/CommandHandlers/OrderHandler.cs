namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers;

public class OrderHandler
    (IOrderRepository orderRepository) 
    : IRequestHandler<CreateCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateCommand command, CancellationToken cancellationToken)
    {
        // Endereço e itens ausentes no JSON chegam como null; viram erros de validação, não exceção.
        // O cartão (command.Card) faz parte do contrato, mas o pagamento é do contexto Payments.
        var address = command.Address ?? new OrderAddressMessageResponse();
        var order = OrderBuilder.Create(customerId: command.UserId)
            .AddAddress(address.Street, address.City, address.State, address.Country, address.ZipCode)
            .AddProduct(command.Items ?? [], CreateOrderItem)
            .Build();

        if (!order.IsValid())
            return CreateOrderResult.Invalid(order.Errors);

        orderRepository.Save(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return CreateOrderResult.Created(order);
    }

    private static Action<OrderItemMessageResponse, OrderBuilder> CreateOrderItem =>
        (item, builder) => builder.CreateItem(item.ProductId, item.ProductName, item.UnitPrice,
            item.Discount, item.PictureUrl, item.Units);
}