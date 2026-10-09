namespace Payments.Domain;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    void Add(Payment payment);
}
