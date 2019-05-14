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

        public Guid CustomerId => _customerId;

        public Address Address { get; private set; }

        #endregion
    }
}
