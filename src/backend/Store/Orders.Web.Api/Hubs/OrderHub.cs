namespace Orders.Web.Api.Hubs;

/// <summary>
/// Mensagem enviada ao navegador: <c>{ orderNumber, approved, status, attempt }</c>. <c>status</c> é o
/// nome do status do pedido (Confirmed, Failed ou NotProcessed); <c>attempt</c> é a tentativa a que o
/// desfecho pertence (a loja ignora a de uma tentativa anterior); <c>approved</c> continua, por
/// compatibilidade.
/// </summary>
public record PaymentStatusChanged(string OrderNumber, bool Approved, string Status, int Attempt)
{
    public static PaymentStatusChanged For(string orderNumber, string status, int attempt)
        => new(orderNumber, status == OrderStatus.Confirmed.Name, status, attempt);
}

/// <summary>
/// O checkout se conecta aqui e chama <see cref="WatchOrder"/> para receber o resultado
/// do pagamento do pedido, sem consultar o pedido periodicamente. Só o dono do pedido,
/// autenticado (token em <c>access_token</c> na query do WebSocket), pode acompanhá-lo.
/// </summary>
[Authorize(Policy = CustomerAuthentication.VerifiedCustomerPolicy)]
public class OrderHub(IMemoryBus bus) : Hub
{
    public const string Path = "/hubs/orders";
    public const string PaymentStatusChangedMethod = "PaymentStatusChanged";
    // A mesma para pedido inexistente e de outro cliente: não revela que o número existe.
    public const string OrderNotFound = "Order not found.";

    public static string GroupName(string orderNumber) => $"order:{orderNumber}";

    public async Task WatchOrder(string orderNumber)
    {
        var customer = CustomerIdentity.From(Context.User!) ?? throw new HubException(OrderNotFound);
        Task<OrderPaymentState?> StatusAsync() => bus.RequestAsync(new GetOrderStatusQuery(orderNumber, customer.CustomerId));

        // Confere o dono antes de entrar no grupo; depois entra e consulta de novo: um resultado
        // que chegue no meio do caminho vem pelo grupo, e um que já tenha chegado vem pela consulta.
        if (await StatusAsync() is null)
            throw new HubException(OrderNotFound);
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(orderNumber));

        if (await StatusAsync() is { } state
            && (state.Status == OrderStatus.Confirmed.Name || state.Status == OrderStatus.Failed.Name
                || state.Status == OrderStatus.NotProcessed.Name))
            await Clients.Caller.SendAsync(PaymentStatusChangedMethod,
                PaymentStatusChanged.For(orderNumber, state.Status, state.Attempt));
    }
}

public class SignalROrderPaymentNotifier(IHubContext<OrderHub> hub) : IOrderPaymentNotifier
{
    public Task NotifyAsync(string orderNumber, OrderStatus status, int attempt,
        CancellationToken cancellationToken = default)
        => hub.Clients.Group(OrderHub.GroupName(orderNumber))
            .SendAsync(OrderHub.PaymentStatusChangedMethod, PaymentStatusChanged.For(orderNumber, status.Name, attempt),
                cancellationToken);
}
