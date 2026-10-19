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

        public Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
            => _context.Orders.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        // O dono entra no filtro da consulta: um pedido de outro cliente nem chega a ser lido.
        // Id desc desempata pedidos da mesma data, para a paginação não repetir nem pular nenhum.
        public async Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(Guid customerId, int skip, int take,
            CancellationToken cancellationToken = default)
            => await CustomerOrders(customerId)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

        public Task<Order?> GetCustomerOrderAsync(Guid customerId, string orderNumber,
            CancellationToken cancellationToken = default)
            => CustomerOrders(customerId).FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        private IQueryable<Order> CustomerOrders(Guid customerId)
            => _context.Orders.AsNoTracking().Include(o => o.Items).Where(o => o.CustomerId == customerId);

        public async Task<IReadOnlyList<Order>> GetAwaitingPaymentEmailAsync(DateTime now, int limit,
            CancellationToken cancellationToken = default)
            // Mesmo critério de Order.IsAwaitingPaymentEmailAt, escrito para o EF traduzir em SQL. Os que
            // nunca falharam vêm antes; depois, os de tentativa vencida há mais tempo.
            => await _context.Orders
                .Where(o => o.Payment.EmailSentAt == null
                            && o.Payment.EmailFailedAt == null
                            && (o.Payment.NextEmailAttemptAt == null || o.Payment.NextEmailAttemptAt <= now)
                            && (o.Status.Code == OrderStatus.Confirmed.Code || o.Status.Code == OrderStatus.Failed.Code
                                || o.Status.Code == OrderStatus.NotProcessed.Code))
                .OrderBy(o => o.Payment.NextEmailAttemptAt == null ? 0 : 1)
                .ThenBy(o => o.Payment.NextEmailAttemptAt)
                .ThenBy(o => o.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<int> CountEmailFailedSinceAsync(DateTime since, CancellationToken cancellationToken = default)
            => _context.Orders.CountAsync(o => o.Payment.EmailFailedAt != null && o.Payment.EmailFailedAt >= since,
                cancellationToken);

        public async Task<IReadOnlyList<Order>> GetPaymentCheckDueAsync(DateTime createdBefore, DateTime checkedBefore,
            int limit, CancellationToken cancellationToken = default)
            // Mesmo critério de Order.IsPaymentCheckDue, escrito para o EF traduzir em SQL.
            => await _context.Orders
                .Where(o => o.Status.Code == OrderStatus.Pending.Code
                            && (o.Payment.AttemptRequestedAt ?? o.OrderDate) <= createdBefore
                            && (o.Payment.StatusCheckedAt == null || o.Payment.StatusCheckedAt <= checkedBefore))
                .OrderBy(o => o.Id)
                .Take(limit)
                .ToListAsync(cancellationToken);

        public Task<int> CountPendingCreatedBeforeAsync(DateTime createdBefore, CancellationToken cancellationToken = default)
            => _context.Orders.CountAsync(o => o.Status.Code == OrderStatus.Pending.Code
                                               && (o.Payment.AttemptRequestedAt ?? o.OrderDate) <= createdBefore,
                cancellationToken);

        public async Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken cancellationToken = default)
        {
            // Sem commit, o DisposeAsync da transação faz o rollback.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            await work();
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
