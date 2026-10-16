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
        // Nada foi cobrado: o Payments anulou o pedido que passou do prazo sem pagamento.
        public static readonly OrderStatus NotProcessed     = new(11, "NotProcessed", "Pagamento não processado");

        public string Name { get; }
        public string Description { get; }
        public int Code { get; }

        private OrderStatus(int code, string name, string description)
        {
            this.Code = code;
            this.Name = name;
            this.Description = description;
        }


        // Igualdade e hash do ValueObject: dois status são iguais quando têm o mesmo código e nome.
        protected override IEnumerable<object> GetValues()
        {
            yield return Code;
            yield return Name;
        }
    }
}