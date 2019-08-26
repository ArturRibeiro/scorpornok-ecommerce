using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Bus;
using Shared.Code.Bus;
using Shared.Code.Notifications;
using Store.Web.Api.App.CommandHandlers;
using Store.Web.Api.App.Commands;

namespace Store.Web.Api.App
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
            services.AddScoped<IRequestHandler<OrderAddressCommand>, OrdersCommandHandler>();
        }
    }
}
