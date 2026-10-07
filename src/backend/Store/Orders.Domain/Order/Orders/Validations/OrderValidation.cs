namespace Orders.Domain.Order.Orders.Validations
{
    public sealed class OrderValidation : AbstractValidator<Order>
    {
        public OrderValidation()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Customer not found.");

            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .NotNull()
                .WithMessage("Order number not found");

            RuleFor(x => x.OrderDate.Date)
                .Equal(DateTime.Now.Date)
                .WithMessage("Current Date incorret.");

            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("Payment code not found.");

            RuleFor(x => x.Address)
                .NotNull()
                .SetValidator(new OrderAddressValidation())
                .WithMessage("Address not found.");

            RuleForEach(x => x.Items)
                .NotNull()
                .SetValidator(new OrderItemValidation())
                .WithMessage("Order item not found.");
        }
    }
}