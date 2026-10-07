namespace Shared.Code.Commands
{
    public abstract class CommandHandler
    {
        protected readonly DomainNotificationHandler _notificationHandler;
        protected readonly IMemoryBus _mediator;

        protected CommandHandler(IMemoryBus mediator, INotificationHandler<DomainNotification> notificationHandler)
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

        protected bool HasErrors(IReadOnlyCollection<string> errors)
        {
            foreach (var error in errors)
                _mediator.RaiseEvent(DomainNotification.Factory.Create(this, error));

            return errors.Any();
        }
    }
}
