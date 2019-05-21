using System;
using System.Threading.Tasks;
using Shared.Code.Commands;
using MediatR;
using System.Threading;
using Order.Web.Api.App.Commands;
using Shared.Code.Notifications;
using Shared.Code.Bus;
using JetBrains.Annotations;

namespace Order.Web.Api.App.CommandHandlers
{
    public class OrdersCommandHandler : CommandHandler,
        IRequestHandler<CreateOrderCommand>
    {

        public OrdersCommandHandler(IMediatorHandler mediator, INotificationHandler<DomainNotification> notificationHandler)
            : base(mediator, notificationHandler)
        {
        }

        public async Task<Unit> Handle([NotNull] CreateOrderCommand message, CancellationToken cancellationToken)
        {
            return Unit.Value;
        }
    }
}
