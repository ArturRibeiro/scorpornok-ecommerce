namespace Shared.Code
{
    public interface IMemoryBus
    {
        Task SendAsync<T>(T command) where T : Message;
        Task RaiseEvent<T>(T @event) where T : Event;
    }
}
