namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers;

public class OrderHandler
    (IOrderRepository orderRepository) 
    : IRequestHandler<CreateCommand>
{
    public async Task Handle(CreateCommand command, CancellationToken cancellationToken)
    {
        var order = OrderBuilder.Create(customerId: Guid.NewGuid())
            .AddAddress(command.Address.Street, command.Address.City, command.Address.State, command.Address.Country, command.Address.ZipCode)
            .AddProduct(command.Items, CreateOrderItem)
            .AddPaymentMethod(command.Card, CreatePaymentMethod)
            .Build();

        orderRepository.Save(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    private static Action<OrderItemMessageResponse, OrderBuilder> CreateOrderItem =>
        (item, builder) => builder.CreateItem(item.ProductId, item.ProductName, item.UnitPrice,
            item.Discount, item.PictureUrl, item.Units);

    private static readonly Func<CreditCardPaymentCommand, PaymentRequest> CreatePaymentMethod 
        = (command) => new PaymentRequest(Amount: 0, CardHolderName: command.CardHolderName,
            CardNumber: command.CardNumber, ExpirationMonth: command.ExpirationMonth,
            ExpirationYear: command.ExpirationYear, Cvv: command.Cvv, Installments: command.Installments);

    private static readonly Func<Order, CreditCardPaymentCommand, PaymentMethod> CreatePaymentRequestToCreatePayment
        = (order, command) => new PaymentMethod(OrderId: order.Id, CardHolderName: command.CardHolderName,
            CardNumber: command.CardNumber, ExpirationMonth: command.ExpirationMonth,
            ExpirationYear: command.ExpirationYear, Cvv: command.Cvv,
            Amount: command.Amount, Installments: command.Installments);
}