using FluentValidation.Results;
using Shared.Code.Notifications;
using MediatR;
using Shared.Code.Bus;

namespace Shared.Code.Commands
{
    public abstract class CommandHandler
    {
        protected readonly DomainNotificationHandler _notificationHandler;
        protected readonly IMediatorHandler _mediator;

        protected CommandHandler(IMediatorHandler mediator, INotificationHandler<DomainNotification> notificationHandler)
        {
            _notificationHandler = (DomainNotificationHandler)notificationHandler;
            _mediator = mediator;
        }

        protected bool HasErrors(ValidationResult result)
        {
            foreach (var error in result.Errors)
                _mediator.RaiseEvent(DomainNotification.Factory.Create(this,  error.ErrorMessage));

            return result.IsValid;
        }
    }
}
