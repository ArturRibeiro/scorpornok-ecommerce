namespace Orders.Domain.Order.Orders
{
    /// <summary>
    /// Quem compra: nome, telefone e CPF informados no checkout, validados com as regras do cadastro.
    /// Não fica guardado no pedido; só segue para o Customers na mensagem do pedido registrado.
    /// </summary>
    public class OrderBuyer
    {
        public string FullName { get; private set; } = string.Empty;
        /// <summary>Só dígitos.</summary>
        public string Phone { get; private set; } = string.Empty;
        /// <summary>Só dígitos.</summary>
        public string Cpf { get; private set; } = string.Empty;

        private OrderBuyer() { }

        /// <summary>Aceita pontuação no telefone e no CPF.</summary>
        public static OrderBuyer Create(string? fullName, string? phone, string? cpf)
            => new()
            {
                FullName = PersonName.Normalize(fullName),
                Phone = global::Shared.Code.Validation.Phone.Normalize(phone),
                Cpf = global::Shared.Code.Validation.Cpf.Normalize(cpf)
            };

        public IReadOnlyList<string> Validate()
            => new OrderBuyerValidation().Validate(this).Errors.Select(e => e.ErrorMessage).ToList();
    }
}
