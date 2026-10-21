namespace Orders.CommandHandlers.Orders;

/// <summary>
/// Envia ao cliente o e-mail com o resultado do pagamento do pedido. A implementação
/// (SMTP) fica na Infrastructure; quem decide quando enviar é o OrderEmailDispatcher.
/// </summary>
public interface IOrderEmailSender
{
    /// <summary>
    /// Conecta (e autentica) ao servidor de e-mail, para os envios de uma rodada saírem pela mesma conexão.
    /// Falha sai como <see cref="EmailDeliveryException"/>, em geral <see cref="EmailDeliveryFailure.Server"/>.
    /// </summary>
    Task<IOrderEmailSession> OpenSessionAsync(CancellationToken cancellationToken = default);
}

/// <summary>Conexão aberta com o servidor de e-mail durante uma rodada; o descarte encerra a conexão.</summary>
public interface IOrderEmailSession : IAsyncDisposable
{
    Task SendPaymentResultAsync(Order order, CancellationToken cancellationToken = default);
}
