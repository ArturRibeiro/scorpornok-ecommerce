namespace Orders.CommandHandlers.Orders.CommandHandlers;

public class OrderHandler(IOrderRepository orderRepository) : IRequestHandler<CreateCommand>
{
    public async Task Handle(CreateCommand command, CancellationToken cancellationToken)
    {
        var order = OrderBuilder.CreateOrder(customerId: Guid.NewGuid(), paymentId: Guid.NewGuid())
            .AddAddress(command.Address.Street, command.Address.City, command.Address.State, command.Address.Country, command.Address.ZipCode)
            .AddProduct(command.Items, AddItemToBuilder)
            .Build();

        orderRepository.Save(order);

        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    private static Action<OrderItemMessageResponse, OrderBuilder> AddItemToBuilder =>
        (item, builder) => builder.CreateItem(item.ProductId,
            item.ProductName,
            item.UnitPrice,
            item.Discount,
            item.PictureUrl,
            item.Units);
}