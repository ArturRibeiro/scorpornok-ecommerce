using System.Collections.Generic;

namespace Store.Domain.Models.Orders
{
    public class OrderStatus
    {
        private readonly List<OrderItem> _orderItems;

        #region Properties
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        public OrderAddress Address { get; private set; }
        #endregion
    }
}
