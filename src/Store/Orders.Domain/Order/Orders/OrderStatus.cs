namespace Orders.Domain.Order.Orders
{
    public class OrderStatus : ValueObject
    {
        public static OrderStatus Submitted = new OrderStatus(1, nameof(Submitted).ToLowerInvariant());
        public static OrderStatus AwaitingValidation = new OrderStatus(2, nameof(AwaitingValidation).ToLowerInvariant());
        public static OrderStatus StockConfirmed = new OrderStatus(3, nameof(StockConfirmed).ToLowerInvariant());
        public static OrderStatus Paid = new OrderStatus(4, nameof(Paid).ToLowerInvariant());
        public static OrderStatus Shipped = new OrderStatus(5, nameof(Shipped).ToLowerInvariant());
        public static OrderStatus Cancelled = new OrderStatus(6, nameof(Cancelled).ToLowerInvariant());

        //protected OrderStatus() { }

        private OrderStatus(int code, string name)
        {
            this.Code = code;
            this.Name = name;
        }

        public int Code { get; private set; }

        public string Name { get; private set; }

        protected override IEnumerable<object> GetAtomicValues()
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            var status = ((OrderStatus)obj);

            return this.Code.Equals(status.Code)
                   && this.Name.Equals(status.Name);
        }
    }
}
