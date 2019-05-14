using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Domain.Models.Orders
{
    public class OrderStatus
    {
        private readonly List<OrderItem> _orderItems;

        #region Properties
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        public Address Address { get; private set; }
        #endregion
    }
}
