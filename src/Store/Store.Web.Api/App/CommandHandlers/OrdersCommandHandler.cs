using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
            if (message.Address == null)
            {
                await _mediator.RaiseEvent(DomainNotification.Factory.Create(message, nameof(message.Address)));
                return Unit.Value;
            }

            if (message.Items == null || message.Items.Length == 0)
            {
                await _mediator.RaiseEvent(DomainNotification.Factory.Create(message, nameof(message.Items)));
                return Unit.Value;
            }

            if (_notificationHandler.HasNotifications)
                return Unit.Value;

            var address = OrderAddress.Factory.Create(message.Address.Street, message.Address.City, message.Address.State, message.Address.Country, message.Address.ZipCode);
            var order = Order.Factory.Create(address, customerId, paymentId);

            //Refactor
            if (!order.IsValid()) await _mediator.RaiseEvent(DomainNotification.Factory.Create(message, "Ocorreram erros"));

            foreach (var item in message.Items)
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
