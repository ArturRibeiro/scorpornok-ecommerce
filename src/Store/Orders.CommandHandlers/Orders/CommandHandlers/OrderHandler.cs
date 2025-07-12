using MediatR;
using Orders.CommandHandlers.Orders.CommandHandlers.Create;

namespace Orders.CommandHandlers.Orders.CommandHandlers
{
    public class OrderHandler : IRequestHandler<CreateCommand>
    {
        public async Task Handle(CreateCommand request, CancellationToken cancellationToken)
        {
            // var customerId = Guid.NewGuid();
            // var paymentId = Guid.NewGuid();
            // var commandValid = ValidCommand(message);
            //
            // if (commandValid.IsNone)
            //     await _mediator.RaiseEvent(DomainNotification.Factory.Create(commandValid.Message, nameof(message.Address)));
            // else
            // {
            //     var address = OrderAddress.Factory.Create(message.Address.Street, message.Address.City,
            //         message.Address.State, message.Address.Country, message.Address.ZipCode);
            //     var order = Order.Factory.Create(address, customerId, paymentId);
            //
            //     foreach (var item in message.Items)
            //         order.AddItem(item.ProductId
            //             , item.ProductName
            //             , item.UnitPrice
            //             , item.Discount
            //             , item.PictureUrl
            //             , item.Units);
            //
            //     if (!order.IsValid())
            //         this.HasErrors(order.Errors);
            //     else
            //     {
            //         this._oderOrderRepository.Save(order);
            //
            //         await this._oderOrderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            //
            //         _logger.LogInformation("----- Creating Order - Order: {@Order}", order);
            //     }
            // }
            //
            // return Unit.Value;
        }
    }
}