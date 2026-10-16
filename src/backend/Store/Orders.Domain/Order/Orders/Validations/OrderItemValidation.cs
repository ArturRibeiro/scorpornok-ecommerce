namespace Orders.Domain.Order.Orders.Validations
{
    public class OrderItemValidation : AbstractValidator<OrderItem>
    {
        public OrderItemValidation()
        {
            RuleFor(x => x.ProductName)
                .NotNull()
                .WithMessage("Informe o nome do produto.");

            RuleFor(x => x.PictureUrl)
                .NotNull()
                .WithMessage("Informe a imagem do produto.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0)
                .WithMessage("O preço do produto deve ser maior que zero.");

            //RuleFor(x => x.Discount);

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("A quantidade deve ser maior que zero.");
        }
    }
}