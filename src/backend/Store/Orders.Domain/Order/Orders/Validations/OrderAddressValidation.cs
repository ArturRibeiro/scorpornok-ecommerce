namespace Orders.Domain.Order.Orders.Validations
{
    // Mesmas regras do endereço do cadastro (Customers): tudo obrigatório, menos o complemento.
    public class OrderAddressValidation : AbstractValidator<OrderAddress>
    {
        public OrderAddressValidation()
        {
            RuleFor(x => x.Street)
                .NotEmpty()
                .WithMessage("Informe a rua.");

            RuleFor(x => x.Number)
                .NotEmpty()
                .WithMessage("Informe o número.");

            RuleFor(x => x.District)
                .NotEmpty()
                .WithMessage("Informe o bairro.");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("Informe a cidade.");

            RuleFor(x => x.State)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Informe a UF.")
                .Must(BrazilianState.IsValid)
                .WithMessage("UF inválida.");

            RuleFor(x => x.ZipCode)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Informe o CEP.")
                .Must(ZipCode.IsValid)
                .WithMessage("CEP inválido: informe 8 dígitos.");
        }
    }
}
