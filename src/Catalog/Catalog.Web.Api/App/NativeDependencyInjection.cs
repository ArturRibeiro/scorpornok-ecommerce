using Catalog.Queries.Products.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Code.Notifications;
using System;
using Catalog.Queries.Products.Queries.Impl;

namespace Catalog.Web.Api.App
{
    public class NativeDependencyInjection
    {
        internal static IServiceProvider _container;

        public static T GetInstance<T>()
            => (T)_container.GetService(typeof(T));


        public static void RegisterServices(IServiceCollection services)
        {
            RegisterQueries(services);
            RegisterDomainEvents(services);
        }

        public static void RegisterQueries(IServiceCollection services)
        {
            services.AddScoped<IProductQueries, ProductQueries>();
        }

        private static void RegisterDomainEvents(IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        }

    }
}
