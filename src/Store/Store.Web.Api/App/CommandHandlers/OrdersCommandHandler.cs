using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Programming.Functional.Options;
using Programming.Functional.Options.Asserts;
using Shared.Code.Bus;
using Shared.Code.Commands;
using Shared.Code.Notifications;
using Store.Domain.Models.Orders;
using Store.Infrastructure.Repositories;
using Store.Web.Api.App.Commands;

namespace Store.Web.Api.App.CommandHandlers
{
    public class OrdersCommandHandler : CommandHandler,
        IRequestHandler<CreateOrderCommand>
    {
        private readonly IOrderRepository _oderOrderRepository;

        public OrdersCommandHandler(IMediatorHandler mediator, INotificationHandler<DomainNotification> notificationHandler, IOrderRepository oderOrderRepository)
            : base(mediator, notificationHandler)
        {
            _oderOrderRepository = oderOrderRepository;
        }

        public async Task<Unit> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
        {
            var customerId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            
            //Refactor
            var address = Assert.IsNotNull(message.Address, "Address null");
            var orderItems = Assert.IsNotNull(message.Items, "items null");
            var orderItemsGreaterThanZero = Assert.IsGreaterThan(0, message.Items, "items null");
            var result = Option.Combine(address, orderItems, orderItemsGreaterThanZero);

           if (result.IsNone)
            {
                await _mediator.RaiseEvent(DomainNotification.Factory.Create(result.Message, nameof(message.Address)));
                return Unit.Value;
            }

            var order = Order.Factory.Create
            (
                address : OrderAddress.Factory.Create(address.Value.Street, address.Value.City, address.Value.State, address.Value.Country, address.Value.ZipCode), 
                customerId: customerId, 
                paymentId: paymentId
            );

            //Refactor
            if (!order.IsValid()) await _mediator.RaiseEvent(DomainNotification.Factory.Create(message, "Ocorreram erros"));

            //if (_notificationHandler.HasNotifications)
            //    return Unit.Value;

            foreach (var item in orderItems.Value)
                order.AddItem(item.ProductId
                    , item.ProductName
                    , item.UnitPrice
                    , item.Discount
                    , item.PictureUrl
                    , item.Units);
            
            this._oderOrderRepository.Save(order);

            await this._oderOrderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
