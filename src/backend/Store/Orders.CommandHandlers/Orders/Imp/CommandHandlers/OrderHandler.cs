namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers;

public class OrderHandler
    (IOrderRepository orderRepository, IIntegrationEventPublisher publisher, ILogger<OrderHandler> logger) 
    : IRequestHandler<CreateCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateCommand command, CancellationToken cancellationToken)
    {
        // Endereço e itens ausentes no JSON chegam como null; viram erros de validação, não exceção.
        var address = command.Address ?? new OrderAddressMessageResponse();
        var order = OrderBuilder.Create(customerId: command.UserId)
            .AddAddress(address.Street, address.City, address.State, address.Country, address.ZipCode)
            .AddProduct(command.Items ?? [], CreateOrderItem)
            .Build();

        if (!order.IsValid())
            return CreateOrderResult.Invalid(order.Errors);

        orderRepository.Save(order);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        await RequestPaymentAsync(order, command.Card, cancellationToken);
        return CreateOrderResult.Created(order);
    }

    // O pagamento é do contexto Payments: o pedido já está salvo, então uma falha aqui
    // não desfaz o pedido. Ele fica Pending (sem outbox, ver design da change).
    private async Task RequestPaymentAsync(Order order, CreditCardPaymentCommand card, CancellationToken cancellationToken)
    {
        if (card is null)
        {
            logger.LogWarning("Pedido {OrderNumber} criado sem cartão; o pagamento não foi solicitado.", order.OrderNumber);
            return;
        }

        try
        {
            // O valor cobrado é o total do pedido, nunca o card.Amount enviado pelo cliente.
            await publisher.PublishAsync(new PaymentRequested(order.Id, order.OrderNumber, order.CustomerId,
                order.Total, card.Installments,
                new CardData(card.CardHolderName, card.CardNumber, card.ExpirationMonth, card.ExpirationYear, card.Cvv)),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao solicitar o pagamento do pedido {OrderNumber}; ele ficará Pending.", order.OrderNumber);
        }
    }

    private static Action<OrderItemMessageResponse, OrderBuilder> CreateOrderItem =>
        (item, builder) => builder.CreateItem(item.ProductId, item.ProductName, item.UnitPrice,
            item.Discount, item.PictureUrl, item.Units);
}