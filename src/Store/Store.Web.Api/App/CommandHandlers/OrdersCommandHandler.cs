using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Shared.Code.Bus;
using Shared.Code.Commands;
using Shared.Code.Notifications;
using Store.Domain.Models.Orders;
using Store.Web.Api.App.Commands;

namespace Store.Web.Api.App.CommandHandlers
{
    public class OrdersCommandHandler : CommandHandler,
        IRequestHandler<CreateOrderCommand>,
        IRequestHandler<OrderAddressCommand>
    {

        public OrdersCommandHandler(IMediatorHandler mediator, INotificationHandler<DomainNotification> notificationHandler)
            : base(mediator, notificationHandler) { }

        public async Task<Unit> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
        {
            var order = Order.Create();

            if (order.IsValid())
            {
                
            }

            return Unit.Value;
        }

        public async Task<Unit> Handle(OrderAddressCommand request, CancellationToken cancellationToken)
        {
            return Unit.Value;
        }
    }
}
