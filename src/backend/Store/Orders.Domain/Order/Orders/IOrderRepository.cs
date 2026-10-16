namespace Orders.Domain.Order.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order Save(Order order);
        Task<Order> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

        // Pedidos com resultado de pagamento cujo e-mail ainda não foi enviado, dos mais antigos aos mais novos.
        Task<IReadOnlyList<Order>> GetAwaitingPaymentEmailAsync(int limit, CancellationToken cancellationToken = default);

        // Pendentes criados até createdBefore cuja última consulta ao Payments (se houve) foi até
        // checkedBefore: o critério de Order.IsPaymentCheckDue, dos mais antigos aos mais novos.
        Task<IReadOnlyList<Order>> GetPaymentCheckDueAsync(DateTime createdBefore, DateTime checkedBefore, int limit,
            CancellationToken cancellationToken = default);

        // Quantos pedidos continuam pendentes desde antes de createdBefore (pedidos presos).
        Task<int> CountPendingCreatedBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default);

        // Executa work numa transação do banco: commit no fim, rollback se work lançar exceção.
        Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken cancellationToken = default);
    }
}
