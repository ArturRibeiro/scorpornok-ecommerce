namespace Shared.Code
{
    public interface IMemoryBus
    {
        Task SendAsync<T>(T command) where T : Message;
        Task<TResponse> RequestAsync<TResponse>(Message<TResponse> command);
        Task RaiseEvent<T>(T @event) where T : Event;
    }
}
