namespace Orders.Domain.Order.Orders
{
    /// <summary>
    /// Endereço de entrega do pedido, no formato do cadastro do cliente. A entrega é sempre no
    /// Brasil: UF em maiúsculas e CEP só com dígitos; o complemento é o único campo opcional.
    /// </summary>
    public class OrderAddress : ValueObject
    {
        public const string Brazil = "Brasil";

        public string Street { get; private set; }
        public string Number { get; private set; }
        public string? Complement { get; private set; }
        public string District { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; } = Brazil;
        public string ZipCode { get; private set; }

        //EF
        protected OrderAddress() { }

        protected override IEnumerable<object> GetValues()
        {
            // Using a yield return statement to return each element one at a time
            yield return Street;
            yield return Number;
            yield return Complement ?? string.Empty;
            yield return District;
            yield return City;
            yield return State;
            yield return Country;
            yield return ZipCode;
        }

        public bool IsValid()
            => new OrderAddressValidation().Validate(this).IsValid;

        #region Factory

        public class Factory
        {
            // O país não vem do cliente: a loja só entrega no Brasil.
            public static OrderAddress Create(string? street, string? number, string? complement, string? district,
                string? city, string? state, string? zipcode)
                => new OrderAddress()
                {
                    Street = street?.Trim(),
                    Number = number?.Trim(),
                    Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim(),
                    District = district?.Trim(),
                    City = city?.Trim(),
                    State = BrazilianState.Normalize(state),
                    Country = Brazil,
                    ZipCode = global::Shared.Code.Validation.ZipCode.Normalize(zipcode)
                };
        }

        #endregion
    }
}
