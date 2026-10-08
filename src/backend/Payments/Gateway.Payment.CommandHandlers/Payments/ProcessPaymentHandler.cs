namespace Gateway.Payment.CommandHandlers.Payments;

public class ProcessPaymentHandler(
    IPaymentRepository paymentRepository,
    IPaymentGateway paymentGateway,
    IIntegrationEventPublisher publisher)
    : IRequestHandler<ProcessPaymentCommand>
{
    public async Task Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        // Entrega "pelo menos uma vez": se o pedido já tem pagamento, não cobra de
        // novo, só republica o resultado registrado.
        var payment = await paymentRepository.GetByOrderNumberAsync(command.OrderNumber, cancellationToken);
        if (payment is null)
        {
            var card = command.Card;
            var result = await paymentGateway.SendProcessPaymentAsync(new PaymentRequest(command.Amount,
                card.CardHolderName, card.CardNumber, card.ExpirationMonth, card.ExpirationYear, card.Cvv,
                command.Installments));

            payment = Domain.Payment.Create(command.OrderId, command.OrderNumber, command.Amount,
                command.Installments, card.CardHolderName, card.CardNumber, result);
            paymentRepository.Add(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        if (payment.Approved)
            await publisher.PublishAsync(new PaymentApproved(payment.OrderNumber, payment.Id), cancellationToken);
        else
            await publisher.PublishAsync(new PaymentRejected(payment.OrderNumber, payment.Reason), cancellationToken);
    }
}
