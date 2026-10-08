namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;

public class OrderPaymentHandlers(
    IOrderRepository orderRepository,
    IOrderPaymentNotifier notifier,
    ILogger<OrderPaymentHandlers> logger)
    : IRequestHandler<ConfirmOrderPaymentCommand>,
      IRequestHandler<FailOrderPaymentCommand>,
      IRequestHandler<GetOrderStatusQuery, string?>
{
    public Task Handle(ConfirmOrderPaymentCommand command, CancellationToken cancellationToken)
        => ApplyAsync(command.OrderNumber, order => order.ConfirmPayment(command.PaymentId), approved: true,
            cancellationToken);

    public Task Handle(FailOrderPaymentCommand command, CancellationToken cancellationToken)
        => ApplyAsync(command.OrderNumber, order => order.FailPayment(), approved: false, cancellationToken);

    public async Task<string?> Handle(GetOrderStatusQuery query, CancellationToken cancellationToken)
        => (await orderRepository.GetByNumberAsync(query.OrderNumber, cancellationToken))?.Status.Name;

    // Salva antes de avisar: a loja nunca recebe um resultado que o pedido ainda não reflete.
    private async Task ApplyAsync(string orderNumber, Func<Order, bool> apply, bool approved,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByNumberAsync(orderNumber, cancellationToken);
        if (order is null)
        {
            // Reentregar não resolve: o pedido não existe neste banco.
            logger.LogWarning("Resultado de pagamento para o pedido {OrderNumber}, que não existe.", orderNumber);
            return;
        }

        if (!apply(order))
            return; // já não estava Pending: resultado repetido ou atrasado

        await orderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        await notifier.NotifyAsync(orderNumber, approved, cancellationToken);
    }
}
