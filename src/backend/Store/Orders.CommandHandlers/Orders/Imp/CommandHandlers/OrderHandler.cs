namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers;

public class OrderHandler
    (IOrderRepository orderRepository) 
    : IRequestHandler<CreateCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateCommand command, CancellationToken cancellationToken)
    {
        // Endereço e itens ausentes no JSON chegam como null; viram erros de validação, não exceção.
        var address = command.Address ?? new OrderAddressMessageResponse();
        var order = OrderBuilder.Create(customerId: command.UserId)
            .AddAddress(address.Street, address.City, address.State, address.Country, address.ZipCode)
            .AddProduct(command.Items ?? [], CreateOrderItem)
            .AddPaymentMethod(command.Card, CreatePaymentMethod)
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