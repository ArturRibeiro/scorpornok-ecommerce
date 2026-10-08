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
    }
}
