namespace Orders.Infrastructure.Consumers;

// Consumidores finos: traduzem a mensagem do Payments em comando e o entregam ao
// handler pelo IMemoryBus, como os endpoints fazem com as requisições HTTP.

public class PaymentApprovedConsumer(IMemoryBus bus) : IConsumer<PaymentApproved>
{
    public Task Consume(ConsumeContext<PaymentApproved> context)
        => bus.SendAsync(new ConfirmOrderPaymentCommand(context.Message.OrderNumber, context.Message.PaymentId));
}

public class PaymentRejectedConsumer(IMemoryBus bus) : IConsumer<PaymentRejected>
{
    public Task Consume(ConsumeContext<PaymentRejected> context)
        => bus.SendAsync(new FailOrderPaymentCommand(context.Message.OrderNumber, context.Message.Reason));
}
