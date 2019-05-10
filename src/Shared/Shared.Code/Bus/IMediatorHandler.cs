using Shared.Code.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Code.Bus
{
    public interface IMediatorHandler
    {
        Task Send<T>(T @command) where T : Message;
        Task RaiseEvent<T>(T @event) where T : Event;
    }
}
