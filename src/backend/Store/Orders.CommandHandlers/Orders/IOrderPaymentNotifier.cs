namespace Orders.CommandHandlers.Orders;

/// <summary>
/// Avisa a loja (navegador) do desfecho do pagamento de um pedido: confirmado, recusado ou não
/// processado. A implementação fica na Web.Api (SignalR); os handlers só conhecem esta interface.
/// </summary>
public interface IOrderPaymentNotifier
{
    /// <param name="attempt">Tentativa a que o desfecho pertence: a loja ignora a de uma tentativa anterior.</param>
    Task NotifyAsync(string orderNumber, OrderStatus status, int attempt, CancellationToken cancellationToken = default);
}
