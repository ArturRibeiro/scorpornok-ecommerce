using Shared.Code.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Code.Events
{
    public interface IEventStore
    {
        void Save<T>(T theEvent) where T : Event;
    }
}
