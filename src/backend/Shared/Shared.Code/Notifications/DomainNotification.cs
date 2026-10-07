namespace Shared.Code.Notifications
{
    public sealed record DomainNotification : Event
    {
        public Guid DomainNotificationId { get; private set; } = Guid.NewGuid();
        public string Description { get; private set; }
        public object Value { get; private set; }

        public static class Factory
        {
            public static DomainNotification Create(object value, string description)
                => new DomainNotification()
                {
                    DomainNotificationId = Guid.NewGuid(),
                    Value = value,
                    Description = description
                };
        }
    }
}
