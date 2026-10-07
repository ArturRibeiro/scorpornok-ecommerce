using Gateway.Payment.Data.EventSourcing.Repository;
using Shared.Code.Commands;
using Shared.Code.Events;
using Newtonsoft.Json;
using System;
using Gateway.Payment.Data.EventSourcing.Repository.EventSourcing;

namespace Gateway.Payment.Data.EventSourcing
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
