namespace Orders.Domain.Order.Orders.Validations
{
    public class OrderAddressValidation : AbstractValidator<OrderAddress>
    {
        public OrderAddressValidation()
        {
            RuleFor(x => x.Street)
                .NotEmpty()
                .WithMessage("Informe o endereço.");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("Informe a cidade.");

            RuleFor(x => x.State)
                .NotEmpty()
                .WithMessage("Informe o estado.");

            RuleFor(x => x.Country)
                .NotEmpty()
                .WithMessage("Informe o país.");

            RuleFor(x => x.ZipCode)
                .NotEmpty()
                .WithMessage("Informe o CEP.");

        }
    }
}