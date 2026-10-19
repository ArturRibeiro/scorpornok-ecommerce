namespace Orders.Domain.Order.Orders.Validations
{
    // Mesmas regras do cadastro (Customers): tudo obrigatório.
    public class OrderBuyerValidation : AbstractValidator<OrderBuyer>
    {
        public OrderBuyerValidation()
        {
            RuleFor(x => x.FullName)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Informe o nome completo.")
                .Must(PersonName.IsValid)
                .WithMessage("Informe nome e sobrenome (até 120 caracteres).");

            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Informe o telefone.")
                .Must(Phone.IsValid)
                .WithMessage("Telefone inválido: informe o DDD e o número, com 10 ou 11 dígitos.");

            RuleFor(x => x.Cpf)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Informe o CPF.")
                .Must(Cpf.IsValid)
                .WithMessage("CPF inválido.");
        }
    }
}
