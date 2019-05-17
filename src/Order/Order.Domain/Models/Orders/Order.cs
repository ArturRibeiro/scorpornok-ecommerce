using Order.Domain.Models.Orders;
using Shared.Code.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Domain.Models.Orders
{
    public class Order : Entity, IAggregateRoot
    {
        private readonly Guid _customerId;

        #region Properties

        public Guid CustomerId { get; private set; }

        public ShippingAddress Address { get; private set; }

        public string OrderNumber { get; private set; }

        public DateTime OrderDate { get; private set; }

        public int PaymentId { get; private set; }

        #endregion
    }
}
