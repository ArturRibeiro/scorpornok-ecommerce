namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create
{
    [DataContract]
    public class OrderAddressMessageResponse
    {
        [DataMember, Required]
        public string Street { get; set; }

        [DataMember, Required]
        public string City { get; set; }

        [DataMember, Required]
        public string State { get; set; }

        [DataMember, Required]
        public string Country { get; set; }

        [DataMember, Required]
        public string ZipCode { get; set; }
    }
}
