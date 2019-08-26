using FluentValidation;

namespace Store.Domain.Models.Orders.Validations
{
    public class OrderAddressValidation : AbstractValidator<OrderAddress>
    {
        public OrderAddressValidation()
        {
            RuleFor(x => x.Street)
                .NotEmpty()
                .NotNull()
                .WithMessage("InvalidOrderAddressStreetEmpty");

            RuleFor(x => x.City)
                .NotEmpty()
                .NotNull()
                .WithMessage("InvalidOrderAddressCityEmpty");

            RuleFor(x => x.State)
                .NotEmpty()
                .NotNull()
                .WithMessage("InvalidOrderAddressStateEmpty");

            RuleFor(x => x.Country)
                .NotEmpty()
                .NotNull()
                .WithMessage("InvalidOrderAddressCountryEmpty");

            RuleFor(x => x.ZipCode)
                .NotEmpty()
                .NotNull()
                .WithMessage("InvalidOrderAddressZipCodeEmpty");

        }
    }
}