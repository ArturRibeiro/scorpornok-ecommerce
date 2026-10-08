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
            .AddEmail(command.Email)
            .AddAddress(address.Street, address.City, address.State, address.Country, address.ZipCode)
            .AddProduct(command.Items ?? [], CreateOrderItem)
            .Build();

        if (!order.IsValid())
            return CreateOrderResult.Invalid(order.Errors);

        // Pedido e solicitação de pagamento na mesma transação: ou os dois ficam gravados, ou nenhum.
        await orderRepository.ExecuteInTransactionAsync(async () =>
        {
            orderRepository.Save(order);
            // Primeiro SaveChanges: gera o order.Id, que vai no PaymentRequested.
            await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            await RequestPaymentAsync(order, command.Card, cancellationToken);
        }, cancellationToken);
        return CreateOrderResult.Created(order);
    }

    // O publicador grava a mensagem no outbox do Orders (não fala com o broker), e ela só é
    // persistida no SaveChanges seguinte. Publicar depois do último SaveChanges perderia a
    // mensagem sem erro. A entrega ao RabbitMQ acontece em segundo plano.
    private async Task RequestPaymentAsync(Order order, CreditCardPaymentCommand card, CancellationToken cancellationToken)
    {
        if (card is null)
        {
            logger.LogWarning("Pedido {OrderNumber} criado sem cartão; o pagamento não foi solicitado.", order.OrderNumber);
            return;
        }

        // O valor cobrado é o total do pedido, nunca o card.Amount enviado pelo cliente.
        await publisher.PublishAsync(new PaymentRequested(order.Id, order.OrderNumber, order.CustomerId,
            order.Total, card.Installments,
            new CardData(card.CardHolderName, card.CardNumber, card.ExpirationMonth, card.ExpirationYear, card.Cvv)),
            cancellationToken);
        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    private static Action<OrderItemMessageResponse, OrderBuilder> CreateOrderItem =>
        (item, builder) => builder.CreateItem(item.ProductId, item.ProductName, item.UnitPrice,
            item.Discount, item.PictureUrl, item.Units);
}