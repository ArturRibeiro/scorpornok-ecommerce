namespace Orders.Domain.Order.Orders
{
    public class OrderItem : Entity<int>
    {
        // DDD Patterns comment
        // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
        // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
        public string ProductName { get; private set; }
        public string PictureUrl { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Discount { get; private set; }
        public int Quantity { get; private set; }

        #region Properties

        public Guid ProductId { get; private set; }

        #endregion

        //protected OrderItem() { }

        private OrderItem(Guid productId, string productName, decimal unitPrice, decimal discount, string PictureUrl, int quantity)
        {
            ProductId = productId;
            ProductName = productName;
            UnitPrice = unitPrice;
            Discount = discount;
            Quantity = quantity;
            this.PictureUrl = PictureUrl;
        }

        

        public static OrderItem Create(Guid productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units)
            => new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
    }
}
