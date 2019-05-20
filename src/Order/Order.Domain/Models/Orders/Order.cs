using Shared.Code.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Order.Domain.Models.Orders
{
    /// <summary>
    /// Pedido
    /// </summary>
    public class Order : Entity, IAggregateRoot
    {
        private readonly IList<OrderItem> _items = new List<OrderItem>();

        #region Properties

        public IReadOnlyCollection<OrderItem> Items => new ReadOnlyCollection<OrderItem>(_items);

        /// <summary>
        /// Endereço de entrega
        /// </summary>
        public OrderAddress Address
        {
            get;
            private set;
        }

        /// <summary>
        /// Cliente que realizou a compra
        /// </summary>
        public Guid CustomerId
        {
            get;
            private set;
        }

        /// <summary>
        /// Código do Pedido
        /// </summary>
        public string OrderNumber
        {
            get;
            private set;
        }

        public DateTime OrderDate
        {
            get;
            private set;
        }

        /// <summary>
        /// Códido da forma como foi paga o pedido
        /// </summary>
        public Guid PaymentId
        {
            get;
            private set;
        }

        #endregion
    }
}
