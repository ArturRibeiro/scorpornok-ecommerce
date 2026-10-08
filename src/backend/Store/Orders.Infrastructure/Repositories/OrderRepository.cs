namespace Orders.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public OrderRepository(OrderContext context)
            => _context = context ?? throw new ArgumentNullException(nameof(context));

        public Order Save(Order order)
            => _context.Orders.Add(order).Entity;

        public Task<Order> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
            => _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        public async Task<IReadOnlyList<Order>> GetAwaitingPaymentEmailAsync(int limit,
            CancellationToken cancellationToken = default)
            // Mesmo critério de Order.IsAwaitingPaymentEmail, escrito para o EF traduzir em SQL.
            => await _context.Orders
                .Where(o => o.PaymentEmailSentAt == null
                            && (o.Status.Code == OrderStatus.Confirmed.Code || o.Status.Code == OrderStatus.Failed.Code))
                .OrderBy(o => o.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public async Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken cancellationToken = default)
        {
            // Sem commit, o DisposeAsync da transação faz o rollback.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await work();
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
