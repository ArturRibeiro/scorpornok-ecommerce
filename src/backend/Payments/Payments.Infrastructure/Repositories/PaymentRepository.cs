namespace Payments.Infrastructure.Repositories;

public class PaymentRepository(PaymentContext context) : IPaymentRepository
{
    public IUnitOfWork UnitOfWork => context;

    public Task<Domain.Payment> GetByIdempotencyKeyAsync(string idempotencyKey,
        CancellationToken cancellationToken = default)
        => context.Payments.FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, cancellationToken);

    public void Add(Domain.Payment payment) => context.Payments.Add(payment);
}
