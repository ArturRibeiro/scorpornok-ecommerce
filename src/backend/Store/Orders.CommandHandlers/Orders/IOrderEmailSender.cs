namespace Orders.CommandHandlers.Orders;

/// <summary>
/// Envia ao cliente o e-mail com o resultado do pagamento do pedido. A implementação
/// (SMTP) fica na Infrastructure; quem decide quando enviar é o OrderEmailDispatcher.
/// </summary>
public interface IOrderEmailSender
{
    Task SendPaymentResultAsync(Order order, CancellationToken cancellationToken = default);
}
