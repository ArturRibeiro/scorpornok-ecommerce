namespace Orders.Domain.Order.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order Save(Order order);
        Task<Order> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    }
}
