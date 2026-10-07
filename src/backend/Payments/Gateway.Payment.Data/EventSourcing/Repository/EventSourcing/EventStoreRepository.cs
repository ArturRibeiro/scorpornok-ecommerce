using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gateway.Payment.Data.Context;
using Shared.Code.Events;

namespace Gateway.Payment.Data.EventSourcing.Repository.EventSourcing
{
    public class EventStoreRepository : IEventStoreRepository
    {
        private readonly EventStoreContext _context;

        public EventStoreRepository(EventStoreContext context)
            => _context = context;

        public IList<StoredEvent> All(Guid aggregateId)
            => _context
            .StoredEvent
            .Where(x => x.AggregateId == aggregateId)
            .ToList();

        public void Store(StoredEvent theEvent)
        {
            _context.StoredEvent.Add(theEvent);
            _context.SaveChanges();
        }

        public void Dispose()
            => _context.Dispose();
    }
}
