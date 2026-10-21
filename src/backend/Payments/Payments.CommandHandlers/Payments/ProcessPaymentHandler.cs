namespace Payments.CommandHandlers.Payments;

public class ProcessPaymentHandler(
    IPaymentRepository paymentRepository,
    ICardTokenRepository cardTokens,
    IPaymentGateway paymentGateway,
    IIntegrationEventPublisher publisher)
    : IRequestHandler<ProcessPaymentCommand>
{
    public const string InvalidTokenReason = "Token do cartão inválido ou vencido.";

    public async Task Handle(ProcessPaymentCommand command, CancellationToken cancellationToken)
    {
        OrderTelemetry.Tag(command.OrderNumber);

        // Entrega "pelo menos uma vez": a chave (pedido + tentativa) garante uma única cobrança por
        // tentativa. Uma nova tentativa do mesmo pedido (outro cartão) tem chave própria e novo pagamento.
        var key = Domain.Payment.KeyFor(command.OrderNumber, command.Attempt);
        var payment = await paymentRepository.GetByIdempotencyKeyAsync(key, cancellationToken);

        if (payment is null)
        {
            var now = DateTime.UtcNow;
            var token = await cardTokens.GetAsync(command.CardToken, cancellationToken);

            // Grava "em processamento" ANTES de cobrar. Se outra entrega gravou primeiro, o índice
            // único rejeita esta gravação; a exceção sobe e a retentativa encontra o registro.
            payment = Domain.Payment.Start(command.OrderId, command.OrderNumber, command.Attempt, command.Amount,
                command.Installments, token?.CardHolderName ?? "", token?.Last4 ?? "");
            paymentRepository.Add(payment);

            if (token is not null && token.CanBeUsed(now))
            {
                // Uso único: o token é marcado na mesma gravação do pagamento. Se outra cobrança o usou
                // ao mesmo tempo, a gravação falha por concorrência e a retentativa recusa.
                token.MarkUsed(now);
            }
            else
            {
                // Desconhecido, vencido ou já usado por outro pagamento: recusa com motivo claro, sem
                // passar pelo gateway e sem erro (nada vai para a fila de erro). O pedido termina Failed.
                payment.Complete(new PaymentResult(false, InvalidTokenReason));
            }
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        if (payment.Status == PaymentStatus.Processing)
        {
            // Com a mesma chave, o gateway devolve o resultado da cobrança original, se houver:
            // uma reentrega depois de cobrar (e antes de gravar o resultado) não cobra de novo.
            // O cartão vai como o pagamento o guardou (titular e 4 últimos dígitos, vindos do token).
            var result = await paymentGateway.SendProcessPaymentAsync(new PaymentRequest(payment.IdempotencyKey,
                command.Amount, payment.CardHolderName, payment.CardLast4, command.Installments));
            payment.Complete(result);
            await paymentRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        // Pagamento anulado (Voided) pela consulta do Orders: a solicitação chegou depois do prazo e
        // é recusada sem passar pelo gateway; Approved é falso e Reason traz o motivo da anulação.
        // O resultado leva a tentativa: o Orders ignora o de uma tentativa que já não é a atual.
        if (payment.Approved)
            await publisher.PublishAsync(new PaymentApproved(payment.OrderNumber, payment.Id, payment.Attempt),
                cancellationToken);
        else
            await publisher.PublishAsync(new PaymentRejected(payment.OrderNumber, payment.Reason, payment.Attempt),
                cancellationToken);
    }
}
