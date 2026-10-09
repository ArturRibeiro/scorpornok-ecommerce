namespace Payments.Infrastructure.EventSourcing
{
    public class SqlEventStore : IEventStore
    {
        private readonly IEventStoreRepository _eventStoreRepository;

        public SqlEventStore(IEventStoreRepository eventSourcingRepository)
            => this._eventStoreRepository = eventSourcingRepository;

        public void Save<T>(T theEvent) where T : Event
        {
            var serializedData = JsonConvert.SerializeObject(theEvent);

            var storedEvent = new StoredEvent(
                theEvent,
                serializedData,
                "User not implementation" //_user.Name
                );

            _eventStoreRepository.Store(storedEvent);
        }
    }
}
