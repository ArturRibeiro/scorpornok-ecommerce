namespace Payments.CommandHandlers.Payments;

/// <summary>
/// Responde ao Orders sobre um pedido que passou do prazo sem resultado. O Payments é a fonte da
/// verdade: com pagamento registrado, republica o resultado; em processamento, avisa que ainda
/// está em andamento; sem pagamento, anula a tentativa para que ela nunca seja cobrada.
/// </summary>
public class CheckPaymentStatusHandler(IPaymentRepository paymentRepository, IIntegrationEventPublisher publisher)
    : IRequestHandler<CheckPaymentStatusCommand>
{
    public async Task Handle(CheckPaymentStatusCommand command, CancellationToken cancellationToken)
    {
        OrderTelemetry.Tag(command.OrderNumber);

        var key = Domain.Payment.KeyFor(command.OrderNumber, command.Attempt);
        var payment = await paymentRepository.GetByIdempotencyKeyAsync(key, cancellationToken);

        if (payment is null)
        {
            // Mesma chave da cobrança: se a solicitação gravar "em processamento" primeiro, o índice
            // único rejeita esta anulação; a exceção sobe e a retentativa encontra o registro.
            payment = Domain.Payment.Void(command.OrderNumber, command.Attempt);
            paymentRepository.Add(payment);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        // Publica com o tipo concreto: o MassTransit roteia pelo tipo da mensagem. Todas levam a tentativa
        // consultada, para o Orders aplicar só a resposta da tentativa atual.
        var orderNumber = payment.OrderNumber;
        var attempt = command.Attempt;
        switch (payment.Status)
        {
            case PaymentStatus.Approved:
                await publisher.PublishAsync(new PaymentApproved(orderNumber, payment.Id, attempt), cancellationToken);
                break;
            case PaymentStatus.Rejected:
                await publisher.PublishAsync(new PaymentRejected(orderNumber, payment.Reason, attempt), cancellationToken);
                break;
            case PaymentStatus.Processing:
                await publisher.PublishAsync(new PaymentStatusPending(orderNumber, attempt), cancellationToken);
                break;
            default:
                await publisher.PublishAsync(new PaymentNotProcessed(orderNumber, attempt), cancellationToken);
                break;
        }
    }
}
