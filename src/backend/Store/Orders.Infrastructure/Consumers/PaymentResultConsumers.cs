namespace Orders.Infrastructure.Consumers;

// Consumidores finos: traduzem a mensagem do Payments em comando e o entregam ao
// handler pelo IMemoryBus, como os endpoints fazem com as requisições HTTP.

public class PaymentApprovedConsumer(IMemoryBus bus) : IConsumer<PaymentApproved>
{
    public Task Consume(ConsumeContext<PaymentApproved> context)
        => bus.SendAsync(new ConfirmOrderPaymentCommand(context.Message.OrderNumber, context.Message.PaymentId,
            context.Message.Attempt));
}

public class PaymentRejectedConsumer(IMemoryBus bus) : IConsumer<PaymentRejected>
{
    public Task Consume(ConsumeContext<PaymentRejected> context)
        => bus.SendAsync(new FailOrderPaymentCommand(context.Message.OrderNumber, context.Message.Reason,
            context.Message.Attempt));
}

public class PaymentNotProcessedConsumer(IMemoryBus bus) : IConsumer<PaymentNotProcessed>
{
    public Task Consume(ConsumeContext<PaymentNotProcessed> context)
        => bus.SendAsync(new MarkOrderPaymentNotProcessedCommand(context.Message.OrderNumber, context.Message.Attempt));
}

// O pagamento ainda está sendo cobrado: nada muda. O PendingOrderWatcher consulta de novo depois do intervalo.
public class PaymentStatusPendingConsumer(ILogger<PaymentStatusPendingConsumer> logger) : IConsumer<PaymentStatusPending>
{
    public Task Consume(ConsumeContext<PaymentStatusPending> context)
    {
        logger.LogInformation(
            "Pagamento do pedido {OrderNumber} (tentativa {Attempt}) ainda em processamento; nova consulta no próximo intervalo.",
            context.Message.OrderNumber, context.Message.Attempt);
        return Task.CompletedTask;
    }
}
