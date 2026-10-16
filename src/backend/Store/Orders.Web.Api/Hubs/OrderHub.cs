using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Domain.Order.Orders;

namespace Orders.Web.Api.Hubs;

/// <summary>
/// Mensagem enviada ao navegador: <c>{ orderNumber, approved, status }</c>. <c>status</c> é o nome do
/// status do pedido (Confirmed, Failed ou NotProcessed); <c>approved</c> continua, por compatibilidade.
/// </summary>
public record PaymentStatusChanged(string OrderNumber, bool Approved, string Status)
{
    public static PaymentStatusChanged For(string orderNumber, string status)
        => new(orderNumber, status == OrderStatus.Confirmed.Name, status);
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
        Task<string?> StatusAsync() => bus.RequestAsync(new GetOrderStatusQuery(orderNumber, customer.CustomerId));

        // Confere o dono antes de entrar no grupo; depois entra e consulta de novo: um resultado
        // que chegue no meio do caminho vem pelo grupo, e um que já tenha chegado vem pela consulta.
        if (await StatusAsync() is null)
            throw new HubException(OrderNotFound);
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(orderNumber));

        var status = await StatusAsync();
        if (status == OrderStatus.Confirmed.Name || status == OrderStatus.Failed.Name
            || status == OrderStatus.NotProcessed.Name)
            await Clients.Caller.SendAsync(PaymentStatusChangedMethod, PaymentStatusChanged.For(orderNumber, status));
    }
}

public class SignalROrderPaymentNotifier(IHubContext<OrderHub> hub) : IOrderPaymentNotifier
{
    public Task NotifyAsync(string orderNumber, OrderStatus status, CancellationToken cancellationToken = default)
        => hub.Clients.Group(OrderHub.GroupName(orderNumber))
            .SendAsync(OrderHub.PaymentStatusChangedMethod, PaymentStatusChanged.For(orderNumber, status.Name),
                cancellationToken);
}
