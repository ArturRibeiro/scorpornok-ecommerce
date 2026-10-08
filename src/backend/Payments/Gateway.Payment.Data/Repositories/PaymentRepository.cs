namespace Gateway.Payment.Data.Repositories;

public class PaymentRepository(PaymentContext context) : IPaymentRepository
{
    public IUnitOfWork UnitOfWork => context;

    public Task<Domain.Payment> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        => context.Payments.FirstOrDefaultAsync(p => p.OrderNumber == orderNumber, cancellationToken);

    public void Add(Domain.Payment payment) => context.Payments.Add(payment);
}
