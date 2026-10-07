namespace Orders.Domain.Order.Orders
{
    public class OrderStatus : ValueObject
    {
        public static readonly OrderStatus Pending          = new(0, "Pending", "Pendente");
        public static readonly OrderStatus Confirmed        = new(1, "Confirmed", "Confirmado");
        public static readonly OrderStatus Processing       = new(2, "Processing", "Em processamento");
        public static readonly OrderStatus Shipped          = new(3, "Shipped", "Enviado");
        public static readonly OrderStatus Delivered        = new(4, "Delivered", "Entregue");
        public static readonly OrderStatus Cancelled        = new(5, "Cancelled", "Cancelado");
        public static readonly OrderStatus Returned         = new(6, "Returned", "Devolvido");
        public static readonly OrderStatus Refunded         = new(7, "Refunded", "Reembolsado");
        public static readonly OrderStatus OnHold           = new(8, "OnHold", "Em espera");
        public static readonly OrderStatus PartiallyShipped = new(9, "PartiallyShipped", "Parcialmente enviado");
        public static readonly OrderStatus Failed           = new(10, "Failed", "Falha no pagamento");

        public string Name { get; }
        public string Description { get; }
        public int Code { get; }

        private OrderStatus(int code, string name, string description)
        {
            this.Code = code;
            this.Name = name;
            this.Description = description;
        }


        protected override IEnumerable<object> GetValues()
        {
            yield return Pending;
            yield return Confirmed;
            yield return Processing;
            yield return Shipped;
            yield return Delivered;
            yield return Cancelled;
            yield return Returned;
            yield return Refunded;
            yield return OnHold;
            yield return PartiallyShipped;
            yield return Failed;
        }

        public override bool Equals(object obj)
        {
            var status = ((OrderStatus)obj);

            return this.Code.Equals(status.Code)
                   && this.Name.Equals(status.Name);
        }
    }
}