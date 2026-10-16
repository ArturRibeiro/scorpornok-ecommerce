namespace Payments.CommandHandlers.Payments;

public class ProcessPaymentHandler(
    IPaymentRepository paymentRepository,
    IPaymentGateway paymentGateway,
    IIntegrationEventPublisher publisher)
    : IRequestHandler<ProcessPaymentCommand>
{
    // Só há uma tentativa por pedido até existir a nova tentativa com outro cartão.
    private const int Attempt = 1;

    public async Task Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        // Entrega "pelo menos uma vez": a chave (pedido + tentativa) garante uma única cobrança.
        var key = Domain.Payment.KeyFor(command.OrderNumber, Attempt);
        var payment = await paymentRepository.GetByIdempotencyKeyAsync(key, cancellationToken);
        var card = command.Card;

        if (payment is null)
        {
            // Grava "em processamento" ANTES de cobrar. Se outra entrega gravou primeiro, o índice
            // único rejeita esta gravação; a exceção sobe e a retentativa encontra o registro.
            payment = Domain.Payment.Start(command.OrderId, command.OrderNumber, Attempt, command.Amount,
                command.Installments, card.CardHolderName, card.CardNumber);
            paymentRepository.Add(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        if (payment.Status == PaymentStatus.Processing)
        {
            // Com a mesma chave, o gateway devolve o resultado da cobrança original, se houver:
            // uma reentrega depois de cobrar (e antes de gravar o resultado) não cobra de novo.
            var result = await paymentGateway.SendProcessPaymentAsync(new PaymentRequest(payment.IdempotencyKey,
                command.Amount, card.CardHolderName, card.CardNumber, card.ExpirationMonth, card.ExpirationYear,
                card.Cvv, command.Installments));
            payment.Complete(result);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        if (payment.Approved)
            await publisher.PublishAsync(new PaymentApproved(payment.OrderNumber, payment.Id), cancellationToken);
        else
            await publisher.PublishAsync(new PaymentRejected(payment.OrderNumber, payment.Reason), cancellationToken);
    }
}
