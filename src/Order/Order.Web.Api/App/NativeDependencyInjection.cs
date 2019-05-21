using Microsoft.Extensions.DependencyInjection;
using System;
using Shared.Code.Bus;
using Shared.Bus;
using MediatR;
using Shared.Code.Notifications;
using Order.Web.Api.App.Commands;
using Order.Web.Api.App.CommandHandlers;
using Shared.Code.Events;
namespace Order.Web.Api.App
{
    public class NativeDependencyInjection
    {
        internal static IServiceProvider _container;

        public static T GetInstance<T>()
            => (T)_container.GetService(typeof(T));


        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IMediatorHandler, InMemoryBus>();

            RegisterEventSourcing(services);
            RegisterDomainEvents(services);
            RegisterCommandHandler(services);
        }
        private static void RegisterEventSourcing(IServiceCollection services)
        {
            //services.AddScoped<IEventStoreRepository, EventStoreRepository>();
            //services.AddScoped<IEventStore, SqlEventStore>();
            //services.AddScoped<EventStoreContext>();
        }

        private static void RegisterDomainEvents(IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        }

        private static void RegisterCommandHandler(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<CreateOrderCommand>, OrdersCommandHandler>();
        }
    }
}
