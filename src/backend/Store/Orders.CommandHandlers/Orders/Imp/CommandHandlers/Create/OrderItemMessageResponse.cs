namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create
{
    [DataContract]
    public class OrderItemMessageResponse
    {
        [DataMember]
        public Guid ProductId { get; set; }

        [DataMember] public string ProductName { get; set; }

        [DataMember]
        public string PictureUrl { get; set; }

        [DataMember]
        public decimal UnitPrice { get; set; }

        [DataMember]
        public decimal Discount { get; set; }

        [DataMember]
        public int Units { get; set; }
    }
}