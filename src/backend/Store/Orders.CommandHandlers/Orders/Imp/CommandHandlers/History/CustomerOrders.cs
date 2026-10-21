namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.History;

/// <summary>
/// Página do histórico de pedidos do cliente (o cliente vem do token de acesso), do mais recente para o
/// mais antigo. <paramref name="Page"/> começa em 1; <paramref name="PageSize"/> é ajustado ao limite.
/// </summary>
public record GetCustomerOrdersQuery(Guid CustomerId, int Page, int PageSize) : Message<CustomerOrderPage>;

/// <summary>
/// Um pedido do histórico, ou nulo se o número não existe ou é de outro cliente: quem pergunta não
/// distingue os dois casos.
/// </summary>
public record GetCustomerOrderQuery(Guid CustomerId, string OrderNumber) : Message<CustomerOrderDetail?>;

// O histórico mostra o pedido como foi comprado: tudo vem do pedido gravado, nada do Catalog ou do Customers.
// Os DTOs listam campo a campo o que sai da API: nada de PaymentId, e-mail, cliente ou estado do e-mail.

/// <summary><paramref name="HasMore"/>: há pedidos depois desta página.</summary>
public record CustomerOrderPage(IReadOnlyList<CustomerOrderSummary> Items, int Page, int PageSize, bool HasMore);

/// <summary>Linha da lista: <paramref name="ItemCount"/> é a soma das quantidades; a foto é a do primeiro item.</summary>
public record CustomerOrderSummary(string OrderNumber, DateTime OrderDate, string Status, decimal Total, int ItemCount,
    string? PictureUrl);

/// <summary>
/// Detalhe do pedido. <paramref name="Status"/> é o <c>OrderStatus.Name</c>; <paramref name="CanRetryPayment"/>
/// diz se o cliente pode pagar de novo com outro cartão (a regra é do <see cref="Order"/>).
/// </summary>
public record CustomerOrderDetail(string OrderNumber, DateTime OrderDate, string Status, decimal Total, int Attempt,
    int MaxAttempts, bool CanRetryPayment, CustomerOrderAddress Address, IReadOnlyList<CustomerOrderItem> Items);

/// <summary>Endereço de entrega do pedido, sem o país (a entrega é sempre no Brasil).</summary>
public record CustomerOrderAddress(string Street, string Number, string? Complement, string District, string City,
    string State, string ZipCode);

/// <summary>Item como foi comprado; <paramref name="Subtotal"/> é preço unitário × quantidade, como o total.</summary>
public record CustomerOrderItem(int ProductId, string ProductName, string? PictureUrl, decimal UnitPrice,
    decimal Discount, int Quantity, decimal Subtotal);

public class CustomerOrdersHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetCustomerOrdersQuery, CustomerOrderPage>,
      IRequestHandler<GetCustomerOrderQuery, CustomerOrderDetail?>
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 50;

    public async Task<CustomerOrderPage> Handle(GetCustomerOrdersQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        // Uma linha a mais diz se há outra página, sem contar todos os pedidos do cliente.
        var orders = await orderRepository.GetCustomerOrdersAsync(query.CustomerId, (page - 1) * pageSize,
            pageSize + 1, cancellationToken);

        var items = orders.Take(pageSize).Select(Summary).ToList();
        return new CustomerOrderPage(items, page, pageSize, orders.Count > pageSize);
    }

    public async Task<CustomerOrderDetail?> Handle(GetCustomerOrderQuery query, CancellationToken cancellationToken)
    {
        // O repositório já filtra pelo cliente; a conferência aqui não deixa passar um pedido de outro.
        if (await orderRepository.GetCustomerOrderAsync(query.CustomerId, query.OrderNumber, cancellationToken) is not { } order
            || order.CustomerId != query.CustomerId)
            return null;
        OrderTelemetry.Tag(order.OrderNumber);
        return Detail(order);
    }

    private static CustomerOrderSummary Summary(Order order)
        => new(order.OrderNumber, order.OrderDate, order.Status.Name, order.Total,
            order.Items.Sum(i => i.Quantity), ItemsInOrder(order).FirstOrDefault()?.PictureUrl);

    private static CustomerOrderDetail Detail(Order order)
        => new(order.OrderNumber, order.OrderDate, order.Status.Name, order.Total, order.Payment.Attempt,
            Order.MaxPaymentAttempts, order.CanRetryPayment,
            new CustomerOrderAddress(order.Address.Street, order.Address.Number, order.Address.Complement,
                order.Address.District, order.Address.City, order.Address.State, order.Address.ZipCode),
            ItemsInOrder(order)
                .Select(i => new CustomerOrderItem(i.ProductId, i.ProductName, i.PictureUrl, i.UnitPrice, i.Discount,
                    i.Quantity, i.UnitPrice * i.Quantity))
                .ToList());

    // Na ordem em que foram gravados: o "primeiro item" da lista é sempre o mesmo.
    private static IEnumerable<OrderItem> ItemsInOrder(Order order) => order.Items.OrderBy(i => i.Id);
}
