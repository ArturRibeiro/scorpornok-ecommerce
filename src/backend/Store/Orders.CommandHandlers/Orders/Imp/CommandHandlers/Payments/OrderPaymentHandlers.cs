namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;

public class OrderPaymentHandlers(
    IOrderRepository orderRepository,
    IOrderPaymentNotifier notifier,
    ILogger<OrderPaymentHandlers> logger)
    : IRequestHandler<ConfirmOrderPaymentCommand>,
      IRequestHandler<FailOrderPaymentCommand>,
      IRequestHandler<MarkOrderPaymentNotProcessedCommand>,
      IRequestHandler<GetOrderStatusQuery, OrderPaymentState?>
{
    public Task Handle(ConfirmOrderPaymentCommand command, CancellationToken cancellationToken)
        => ApplyAsync(command.OrderNumber, order => order.ConfirmPayment(command.PaymentId, command.Attempt), cancellationToken);

    public Task Handle(FailOrderPaymentCommand command, CancellationToken cancellationToken)
        => ApplyAsync(command.OrderNumber, order => order.FailPayment(command.Attempt), cancellationToken);

    public Task Handle(MarkOrderPaymentNotProcessedCommand command, CancellationToken cancellationToken)
        => ApplyAsync(command.OrderNumber, order => order.MarkPaymentNotProcessed(command.Attempt), cancellationToken);

    public async Task<OrderPaymentState?> Handle(GetOrderStatusQuery query, CancellationToken cancellationToken)
        => await orderRepository.GetByNumberAsync(query.OrderNumber, cancellationToken) is { } order
           && order.CustomerId == query.CustomerId
            ? new OrderPaymentState(order.Status.Name, order.Payment.Attempt)
            : null;

    // Salva antes de avisar: a loja nunca recebe um resultado que o pedido ainda não reflete.
    private async Task ApplyAsync(string orderNumber, Func<Order, bool> apply, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByNumberAsync(orderNumber, cancellationToken);
        if (order is null)
        {
            // Reentregar não resolve: o pedido não existe neste banco.
            logger.LogWarning("Resultado de pagamento para o pedido {OrderNumber}, que não existe.", orderNumber);
            return;
        }

        if (!apply(order))
            return; // já não estava Pending, ou é de outra tentativa: resultado repetido ou atrasado

        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        await notifier.NotifyAsync(orderNumber, order.Status, order.Payment.Attempt, cancellationToken);
    }
}
