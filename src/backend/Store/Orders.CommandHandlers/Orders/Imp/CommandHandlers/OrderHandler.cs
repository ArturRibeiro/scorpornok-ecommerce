namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers;

public class OrderHandler
    (IOrderRepository orderRepository, IIntegrationEventPublisher publisher, ICatalogProducts catalog,
        ILogger<OrderHandler> logger) 
    : IRequestHandler<CreateCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateCommand command, CancellationToken cancellationToken)
    {
        // Endereço e itens ausentes no JSON chegam como null; viram erros de validação, não exceção.
        var address = command.Address ?? new OrderAddressMessageResponse();
        var items = command.Items ?? [];

        // Preço, nome e foto vêm do Catalog, nunca do navegador. Consulta fora da transação:
        // com o Catalog fora, nada é gravado nem cobrado.
        IReadOnlyDictionary<int, CatalogProduct> products = new Dictionary<int, CatalogProduct>();
        try
        {
            if (items.Count > 0)
                products = await catalog.GetAsync(items.Select(item => item.ProductId).ToArray(), cancellationToken);
        }
        catch (CatalogUnavailableException ex)
        {
            logger.LogWarning(ex, "Catalog indisponível; o pedido não foi registrado.");
            return CreateOrderResult.Unavailable("O catálogo está indisponível. Tente de novo em instantes.");
        }

        var missing = items.Where(item => !products.ContainsKey(item.ProductId))
            .Select(item => item.ProductId).Distinct()
            .Select(productId => $"Produto {productId} não encontrado.")
            .ToList();

        var order = OrderBuilder.Create(customerId: command.UserId)
            .AddEmail(command.Email)
            .AddAddress(address.Street, address.City, address.State, address.Country, address.ZipCode)
            .AddProduct(items.Where(item => products.ContainsKey(item.ProductId)).ToList(),
                (item, builder) => CreateOrderItem(item, products[item.ProductId], builder))
            .Build();

        // IsValid() preenche order.Errors; por isso vem antes de juntar as mensagens.
        if (!order.IsValid() || missing.Count > 0)
            return CreateOrderResult.Invalid([.. missing, .. order.Errors]);

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

    // Do pedido só vale a quantidade; o desconto continua fora do total (e do contrato confiável).
    private static void CreateOrderItem(OrderItemMessageResponse item, CatalogProduct product, OrderBuilder builder)
        => builder.CreateItem(product.Id, product.Name, product.Price, 0m, product.PictureUrl, item.Units);
}