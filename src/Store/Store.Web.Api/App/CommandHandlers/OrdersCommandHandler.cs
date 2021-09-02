using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CreateOrderCommand> _logger;

        public OrdersCommandHandler(IMediatorHandler mediator
            , INotificationHandler<DomainNotification> notificationHandler
            , IOrderRepository oderOrderRepository
            , ILogger<CreateOrderCommand> logger)
            : base(mediator, notificationHandler)
        {
            _oderOrderRepository = oderOrderRepository;
            _logger = logger;
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
                var address = OrderAddress.Factory.Create(message.Address.Street, message.Address.City,
                    message.Address.State, message.Address.Country, message.Address.ZipCode);
                var order = Order.Factory.Create(address, customerId, paymentId);

                foreach (var item in message.Items)
                    order.AddItem(item.ProductId
                        , item.ProductName
                        , item.UnitPrice
                        , item.Discount
                        , item.PictureUrl
                        , item.Units);

                if (!order.IsValid())
                    this.HasErrors(order.Errors);
                else
                {
                    this._oderOrderRepository.Save(order);

                    await this._oderOrderRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

                    _logger.LogInformation("----- Creating Order - Order: {@Order}", order);
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