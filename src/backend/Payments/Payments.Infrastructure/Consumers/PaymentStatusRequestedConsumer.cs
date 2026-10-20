namespace Payments.Infrastructure.Consumers;

/// <summary>Recebe do Orders a consulta da situação do pagamento e a entrega ao handler pelo IMemoryBus.</summary>
public class PaymentStatusRequestedConsumer(IMemoryBus bus) : IConsumer<PaymentStatusRequested>
{
    public Task Consume(ConsumeContext<PaymentStatusRequested> context)
        => bus.SendAsync(new CheckPaymentStatusCommand(context.Message.OrderNumber, context.Message.Attempt));
}
