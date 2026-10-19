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
            .AddAddress(address.Street, address.Number, address.Complement, address.District, address.City, address.State,
                address.ZipCode)
            .AddProduct(items.Where(item => products.ContainsKey(item.ProductId)).ToList(),
                (item, builder) => CreateOrderItem(item, products[item.ProductId], builder))
            .Build();

        // Comprador obrigatório e validado como no cadastro; não fica no pedido, só segue na mensagem.
        var buyer = OrderBuyer.Create(command.Customer?.FullName, command.Customer?.Phone, command.Customer?.Cpf);
        var buyerErrors = buyer.Validate();

        // Pagamento informado sem token: o cartão não foi tokenizado (o Orders nunca recebe o cartão).
        List<string> paymentErrors = command.Payment is { } payment && string.IsNullOrWhiteSpace(payment.CardToken)
            ? ["Informe o cartão."]
            : [];

        // IsValid() preenche order.Errors; por isso vem antes de juntar as mensagens.
        if (!order.IsValid() || missing.Count > 0 || buyerErrors.Count > 0 || paymentErrors.Count > 0)
            return CreateOrderResult.Invalid([.. missing, .. order.Errors, .. buyerErrors, .. paymentErrors]);

        // Pedido, mensagem para o Customers e solicitação de pagamento na mesma transação: ou tudo fica
        // gravado, ou nada.
        await orderRepository.ExecuteInTransactionAsync(async () =>
        {
            orderRepository.Save(order);
            // Primeiro SaveChanges: gera o order.Id, que vai no PaymentRequested.
            await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            // O publicador grava as mensagens no outbox do Orders (não fala com o broker), e elas só são
            // persistidas no SaveChanges seguinte. Publicar depois do último SaveChanges perderia a
            // mensagem sem erro. A entrega ao RabbitMQ acontece em segundo plano.
            await publisher.PublishAsync(OrderPlacedMessage(order, buyer), cancellationToken);
            await RequestPaymentAsync(order, command.Payment, cancellationToken);
            await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }, cancellationToken);
        return CreateOrderResult.Created(order);
    }

    // Para o histórico de endereços do cliente e, na primeira compra, o cadastro.
    private static OrderPlaced OrderPlacedMessage(Order order, OrderBuyer buyer)
        => new(order.OrderNumber, order.CustomerId, order.Email, buyer.FullName, buyer.Phone, buyer.Cpf,
            new ShippingAddress(order.Address.Street, order.Address.Number, order.Address.Complement,
                order.Address.District, order.Address.City, order.Address.State, order.Address.ZipCode),
            order.OrderDate);

    private async Task RequestPaymentAsync(Order order, PaymentMethod? payment, CancellationToken cancellationToken)
    {
        if (payment is null)
        {
            logger.LogWarning("Pedido {OrderNumber} criado sem pagamento; a cobrança não foi solicitada.", order.OrderNumber);
            return;
        }

        // O valor cobrado é o total do pedido; do cartão, só o token (nada de número nem CVV no outbox).
        await publisher.PublishAsync(new PaymentRequested(order.Id, order.OrderNumber, order.CustomerId,
            order.Total, payment.Installments, payment.CardToken!), cancellationToken);
    }

    // Do pedido só vale a quantidade; o desconto continua fora do total (e do contrato confiável).
    private static void CreateOrderItem(OrderItemMessageResponse item, CatalogProduct product, OrderBuilder builder)
        => builder.CreateItem(product.Id, product.Name, product.Price, 0m, product.PictureUrl, item.Units);
}