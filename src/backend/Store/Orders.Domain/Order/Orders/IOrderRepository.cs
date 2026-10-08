namespace Orders.Domain.Order.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order Save(Order order);
        Task<Order> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

        // Pedidos com resultado de pagamento cujo e-mail ainda não foi enviado, dos mais antigos aos mais novos.
        Task<IReadOnlyList<Order>> GetAwaitingPaymentEmailAsync(int limit, CancellationToken cancellationToken = default);

        // Executa work numa transação do banco: commit no fim, rollback se work lançar exceção.
        Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken cancellationToken = default);
    }
}
