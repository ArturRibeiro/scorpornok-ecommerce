using Shared.Code.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gateway.Payment.Shared.Domain.Models.Orders
{
    public class Order : Entity, IAggregateRoot
    {
        #region Properties
        public Address Address { get; private set; }

        #endregion
    }
}
