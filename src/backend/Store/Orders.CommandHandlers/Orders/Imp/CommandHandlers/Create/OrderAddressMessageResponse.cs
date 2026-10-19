namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create
{
    /// <summary>Endereço de entrega no formato do cadastro. Um <c>country</c> no corpo é ignorado (só Brasil).</summary>
    [DataContract]
    public class OrderAddressMessageResponse
    {
        [DataMember, Required]
        public string? Street { get; set; }

        [DataMember, Required]
        public string? Number { get; set; }

        [DataMember]
        public string? Complement { get; set; }

        [DataMember, Required]
        public string? District { get; set; }

        [DataMember, Required]
        public string? City { get; set; }

        [DataMember, Required]
        public string? State { get; set; }

        [DataMember, Required]
        public string? ZipCode { get; set; }
    }
}
