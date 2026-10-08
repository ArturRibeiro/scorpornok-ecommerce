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

            // O pedido nasce Pending, antes do pagamento, então o PaymentId
            // ainda não existe na criação.

            RuleFor(x => x.Address)
                .NotNull()
                .SetValidator(new OrderAddressValidation())
                .WithMessage("Address not found.");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Order has no items.");

            RuleForEach(x => x.Items)
                .NotNull()
                .SetValidator(new OrderItemValidation())
                .WithMessage("Order item not found.");
        }
    }
}