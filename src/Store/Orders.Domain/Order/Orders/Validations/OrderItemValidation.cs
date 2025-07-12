namespace Orders.Domain.Order.Orders.Validations
{
    public class OrderItemValidation : AbstractValidator<OrderItem>
    {
        public OrderItemValidation()
        {
            RuleFor(x => x.ProductName)
                .NotNull()
                .WithMessage("Product name not found.");

            RuleFor(x => x.PictureUrl)
                .NotNull()
                .WithMessage("Picture not found.");

            RuleFor(x => x.UnitPrice)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Unit Price not found.");

            //RuleFor(x => x.Discount);

            RuleFor(x => x.Units)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Units  not found."); ;
        }
    }
}