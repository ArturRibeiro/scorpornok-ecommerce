namespace Shared.Code.Notifications
{
    public class DomainNotificationHandler : INotificationHandler<DomainNotification>
    {
        private List<DomainNotification> _notifications;

        public DomainNotificationHandler()
            => _notifications = new List<DomainNotification>();

        public Task Handle(DomainNotification message, CancellationToken cancellationToken)
        {
            _notifications.Add(message);

            return Task.CompletedTask;
        }

        public List<DomainNotification> GetNotifications() => _notifications;

        public bool HasNotifications => GetNotifications().Any();
    }
}
