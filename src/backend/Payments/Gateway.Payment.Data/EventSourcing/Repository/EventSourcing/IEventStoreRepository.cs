using System;
using System.Collections.Generic;
using Shared.Code.Events;

namespace Gateway.Payment.Data.EventSourcing.Repository.EventSourcing
{
    public interface IEventStoreRepository : IDisposable
    {
        void Store(StoredEvent theEvent);
        IList<StoredEvent> All(Guid aggregateId);
    }
}
