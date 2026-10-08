namespace Orders.CommandHandlers.Orders;

/// <summary>
/// Avisa a loja (navegador) do resultado do pagamento de um pedido. A implementação
/// fica na Web.Api (SignalR); os handlers só conhecem esta interface.
/// </summary>
public interface IOrderPaymentNotifier
{
    Task NotifyAsync(string orderNumber, bool approved, CancellationToken cancellationToken = default);
}
