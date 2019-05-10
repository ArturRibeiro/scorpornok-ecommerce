using Shared.Code.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gateway.Payment.Data.EventSourcing.Repository
{
    public interface IEventStoreRepository : IDisposable
    {
        void Store(StoredEvent theEvent);
        IList<StoredEvent> All(Guid aggregateId);
    }
}
