namespace Payments.Domain;

public interface IPaymentRepository : IRepository<Payment>
{
    /// <summary>O pagamento de uma tentativa (pedido + tentativa); nulo se ainda não foi registrado.</summary>
    Task<Payment> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    void Add(Payment payment);
}
