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
            var commandValid = ValidCommand(message);

            if (commandValid.IsNone)
                await _mediator.RaiseEvent(DomainNotification.Factory.Create(commandValid.Message, nameof(message.Address)));
            else
            {
                var address = OrderAddress.Factory.Create(message.Address.Street, message.Address.City, message.Address.State, message.Address.Country, message.Address.ZipCode);
                var order = Order.Factory.Create(address: address, customerId: customerId, paymentId: paymentId);

                if (!order.IsValid())
                    await _mediator.RaiseEvent(DomainNotification.Factory.Create(order, "Ocorreram erros"));
                else
                {
                    foreach (var item in message.Items)
                        order.AddItem(item.ProductId
                            , item.ProductName
                            , item.UnitPrice
                            , item.Discount
                            , item.PictureUrl
                            , item.Units);

                    this._oderOrderRepository.Save(order);

                    await this._oderOrderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                }
            }

            return Unit.Value;
        }

        private Option<CreateOrderCommand> ValidCommand(CreateOrderCommand message)
            => Option<CreateOrderCommand>.Some(message)
                .OnSuccess(x => Assert.IsNotNull(x.Items, "items null"))
                .OnSuccess(x => Assert.IsGreaterThan(0, x.Items, "items null"))
                .OnSuccess(x => Assert.IsNotNull(x.Address, "items null"));
    }
}
