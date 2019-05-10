using Shared.Code.Commands;
using Shared.Code.Events;
using MediatR;
using Shared.Code.Bus;
using System.Threading.Tasks;

namespace Gateway.Payment.Cross.Bus
{
    public sealed class InMemoryBus : IMediatorHandler
    {
        private readonly IMediator _mediator;
        private readonly IEventStore _eventStore;

        public InMemoryBus(IMediator mediator, IEventStore eventStore)
        {
            this._mediator = mediator;
            this._eventStore = eventStore;
        }

        public async Task RaiseEvent<T>(T @event) where T : Event
        {
            if (!@event.MessageType.Equals("DomainNotification"))
                _eventStore?.Save(@event);

            await _mediator.Publish(@event);
        }

        public async Task Send<T>(T @command) where T : Message
            => await _mediator.Send(@command);
    }
}
