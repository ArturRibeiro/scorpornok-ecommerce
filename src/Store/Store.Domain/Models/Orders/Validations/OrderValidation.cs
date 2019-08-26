using System;
using FluentValidation;

namespace Store.Domain.Models.Orders.Validations
{
    public sealed class OrderValidation : AbstractValidator<Order>
    {
        public OrderValidation()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("InvalidCustomerIdEmpty");

            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .NotNull()
                .WithMessage("InvalidOrderNumberEmptyOrNull");

            RuleFor(x => x.OrderDate.Date)
                .Equal(DateTime.Now.Date)
                .WithMessage("InvalidOrderDataEqual");

            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("InvalidPaymentIdEmpty");

            RuleFor(x => x.Address)
                .NotNull()
                .SetValidator(new OrderAddressValidation());
        }
    }
}