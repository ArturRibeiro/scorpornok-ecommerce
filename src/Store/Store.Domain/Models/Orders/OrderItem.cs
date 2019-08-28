using System;
using Shared.Code.Models;

namespace Store.Domain.Models.Orders
{
    public class OrderItem : Entity<int>
    {
        // DDD Patterns comment
        // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
        // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
        private string _productName;
        private string _pictureUrl;
        private decimal _unitPrice;
        private decimal _discount;
        private int _units;

        #region Properties

        public Guid ProductId { get; private set; }

        #endregion

        //protected OrderItem() { }

        private OrderItem(Guid productId, string productName, decimal unitPrice, decimal discount, string PictureUrl, int units = 1)
        {
            ProductId = productId;
            _productName = productName;
            _unitPrice = unitPrice;
            _discount = discount;
            _units = units;
            _pictureUrl = PictureUrl;
        }

        public static OrderItem Create(Guid productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units)
            => new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
    }
}
