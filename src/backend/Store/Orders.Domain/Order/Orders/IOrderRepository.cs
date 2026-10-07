namespace Orders.Domain.Order.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Order Save(Order order);
    }
}
