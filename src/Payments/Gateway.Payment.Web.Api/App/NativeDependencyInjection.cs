using System;
using Gateway.Payment.Data.Context;
using Gateway.Payment.Data.EventSourcing;
using Gateway.Payment.Data.EventSourcing.Repository.EventSourcing;
using Gateway.Payment.Web.Api.App.eRede;
using Gateway.Payment.Web.Api.App.eRede.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Bus;
using Shared.Code.Bus;
using Shared.Code.Events;
using Shared.Code.Notifications;

namespace Gateway.Payment.Web.Api.App
{
    public class NativeDependencyInjection
    {
        internal static IServiceProvider _container;

        public static T GetInstance<T>()
            => (T)_container.GetService(typeof(T));


        public static void RegisterServices(IServiceCollection services)
        {
            // Domain Bus (Mediator)
            services.AddScoped<IMediatorHandler, InMemoryBus>();

            RegisterCommandHandler(services);
            RegisterDomainEvents(services);
            RegisterEventSourcing(services);
        }

        private static void RegisterEventSourcing(IServiceCollection services)
        {
            services.AddScoped<IEventStoreRepository, EventStoreRepository>();
            services.AddScoped<IEventStore, SqlEventStore>();
            services.AddScoped<EventStoreContext>();
        }

        private static void RegisterCommandHandler(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<AuthorizationCommand>, eRedeCommandHandler>();
        }
        private static void RegisterDomainEvents(IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        }
    }
}
