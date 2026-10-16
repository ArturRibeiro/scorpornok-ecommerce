namespace Orders.Domain.Order.Orders.Validations
{
    public sealed class OrderValidation : AbstractValidator<Order>
    {
        public OrderValidation()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("Cliente não identificado.");

            RuleFor(x => x.OrderNumber)
                .NotEmpty()
                .WithMessage("Número do pedido não gerado.");

            // Para onde vai o e-mail com o resultado do pagamento.
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("E-mail inválido.")
                .EmailAddress()
                .WithMessage("E-mail inválido.");

            RuleFor(x => x.OrderDate.Date)
                .Equal(DateTime.Now.Date)
                .WithMessage("Data do pedido inválida.");

            // O pedido nasce Pending, antes do pagamento, então o Payment.PaymentId
            // ainda não existe na criação.

            RuleFor(x => x.Address)
                .NotNull()
                .WithMessage("Informe o endereço de entrega.")
                .SetValidator(new OrderAddressValidation());

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("O pedido não tem itens.");

            RuleForEach(x => x.Items)
                .NotNull()
                .WithMessage("Item do pedido inválido.")
                .SetValidator(new OrderItemValidation());
        }
    }
}