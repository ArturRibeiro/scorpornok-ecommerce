using Microsoft.AspNetCore.SignalR;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Domain.Order.Orders;

namespace Orders.Web.Api.Hubs;

/// <summary>Mensagem enviada ao navegador: <c>{ orderNumber, approved }</c>.</summary>
public record PaymentStatusChanged(string OrderNumber, bool Approved);

/// <summary>
/// O checkout se conecta aqui e chama <see cref="WatchOrder"/> para receber o resultado
/// do pagamento do pedido, sem consultar o pedido periodicamente.
/// </summary>
public class OrderHub(IMemoryBus bus) : Hub
{
    public const string Path = "/hubs/orders";
    public const string PaymentStatusChangedMethod = "PaymentStatusChanged";

    public static string GroupName(string orderNumber) => $"order:{orderNumber}";

    public async Task WatchOrder(string orderNumber)
    {
        // Entra no grupo antes de consultar: um resultado que chegue no meio do caminho
        // vem pelo grupo, e um que já tenha chegado vem pela consulta.
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(orderNumber));

        var status = await bus.RequestAsync(new GetOrderStatusQuery(orderNumber));
        if (status == OrderStatus.Confirmed.Name || status == OrderStatus.Failed.Name)
            await Clients.Caller.SendAsync(PaymentStatusChangedMethod,
                new PaymentStatusChanged(orderNumber, status == OrderStatus.Confirmed.Name));
    }
}

public class SignalROrderPaymentNotifier(IHubContext<OrderHub> hub) : IOrderPaymentNotifier
{
    public Task NotifyAsync(string orderNumber, bool approved, CancellationToken cancellationToken = default)
        => hub.Clients.Group(OrderHub.GroupName(orderNumber))
            .SendAsync(OrderHub.PaymentStatusChangedMethod, new PaymentStatusChanged(orderNumber, approved), cancellationToken);
}
