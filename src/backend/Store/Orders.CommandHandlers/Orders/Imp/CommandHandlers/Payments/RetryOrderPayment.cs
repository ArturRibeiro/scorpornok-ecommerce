namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;

/// <summary>
/// Nova tentativa de pagamento de um pedido recusado, com outro cartão (já tokenizado no navegador).
/// O cliente vem do token de acesso, como no registro do pedido.
/// </summary>
public record RetryOrderPaymentCommand(string OrderNumber, Guid CustomerId, PaymentMethod? Payment)
    : Message<RetryOrderPaymentResult>;

public enum RetryOrderPaymentOutcome
{
    /// <summary>Pedido de volta a pendente; a nova solicitação de pagamento foi gravada no outbox.</summary>
    Accepted,
    /// <summary>Cartão não informado.</summary>
    Invalid,
    /// <summary>Pedido inexistente ou de outro cliente (quem pergunta não distingue os dois).</summary>
    NotFound,
    /// <summary>O pedido não está recusado ou as tentativas acabaram.</summary>
    Conflict
}

public record RetryOrderPaymentResult(
    RetryOrderPaymentOutcome Outcome,
    string? OrderNumber,
    string? Status,
    int Attempt,
    IReadOnlyCollection<string> Errors)
{
    public static RetryOrderPaymentResult Accepted(Order order)
        => new(RetryOrderPaymentOutcome.Accepted, order.OrderNumber, order.Status.Name, order.Payment.Attempt, []);

    public static RetryOrderPaymentResult Invalid(string error)
        => new(RetryOrderPaymentOutcome.Invalid, null, null, 0, [error]);

    public static readonly RetryOrderPaymentResult NotFound
        = new(RetryOrderPaymentOutcome.NotFound, null, null, 0, ["Pedido não encontrado."]);

    public static RetryOrderPaymentResult Conflict(string error)
        => new(RetryOrderPaymentOutcome.Conflict, null, null, 0, [error]);
}

public class RetryOrderPaymentHandler(
    IOrderRepository orderRepository,
    IIntegrationEventPublisher publisher,
    ILogger<RetryOrderPaymentHandler> logger)
    : IRequestHandler<RetryOrderPaymentCommand, RetryOrderPaymentResult>
{
    public async Task<RetryOrderPaymentResult> Handle(RetryOrderPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByNumberAsync(command.OrderNumber, cancellationToken);
        if (order is null || order.CustomerId != command.CustomerId)
            return RetryOrderPaymentResult.NotFound;

        // O cartão nunca chega ao Orders: o navegador o troca por um token novo a cada tentativa.
        if (command.Payment is not { CardToken: { Length: > 0 } cardToken } payment)
            return RetryOrderPaymentResult.Invalid("Informe o cartão.");

        if (!order.CanRetryPayment)
            return RetryOrderPaymentResult.Conflict(order.Status.Code == OrderStatus.Failed.Code
                ? $"As {Order.MaxPaymentAttempts} tentativas de pagamento do pedido acabaram."
                : "Só é possível tentar de novo o pagamento de um pedido recusado.");

        // Pedido de volta a pendente e nova solicitação de pagamento na mesma transação (outbox), como no
        // registro: ou os dois ficam gravados, ou nenhum. O valor é o total do pedido, que não muda.
        await orderRepository.ExecuteInTransactionAsync(async () =>
        {
            order.RetryPayment(DateTime.Now);
            await publisher.PublishAsync(new PaymentRequested(order.Id, order.OrderNumber, order.CustomerId,
                order.Total, payment.Installments, cardToken, order.Payment.Attempt), cancellationToken);
            await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }, cancellationToken);

        logger.LogInformation("Pedido {OrderNumber}: nova tentativa de pagamento ({Attempt} de {Max}).",
            order.OrderNumber, order.Payment.Attempt, Order.MaxPaymentAttempts);
        return RetryOrderPaymentResult.Accepted(order);
    }
}
