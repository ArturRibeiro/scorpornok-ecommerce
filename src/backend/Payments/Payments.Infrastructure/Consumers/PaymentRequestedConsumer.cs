using MassTransit;

namespace Payments.Infrastructure.Consumers;

/// <summary>Recebe a solicitação do Orders e a entrega ao handler pelo IMemoryBus.</summary>
public class PaymentRequestedConsumer(IMemoryBus bus) : IConsumer<PaymentRequested>
{
    public Task Consume(ConsumeContext<PaymentRequested> context)
    {
        var message = context.Message;
        return bus.SendAsync(new ProcessPaymentCommand(message.OrderId, message.OrderNumber, message.Amount,
            message.Installments, message.Card));
    }
}
