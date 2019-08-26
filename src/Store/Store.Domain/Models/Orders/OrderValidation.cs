using FluentValidation;

namespace Store.Domain.Models.Orders
{
    public sealed class OrderValidation : AbstractValidator<Store.Domain.Models.Orders.Order>
    {
        public OrderValidation()
        {
            
        }
    }
}