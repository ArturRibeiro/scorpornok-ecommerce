using Catalog.Queries.Products.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Code.Notifications;
using System;
using Catalog.Queries.Products.Queries.Impl;
using Frameworker.Scorponok.Reading.Database;

namespace Catalog.Web.Api.App
{
    public static class NativeDependencyInjection
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            RegisterQueries(services);
            RegisterDomainEvents(services);
            ApplicationReadQueryServices(services);

            return services;
        }

        private static void RegisterQueries(IServiceCollection services) => services.AddScoped<IProductQueries, ProductQueries>();

        private static void RegisterDomainEvents(IServiceCollection services) => services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        private static void ApplicationReadQueryServices(IServiceCollection services) => services.AddReadDbScoped();
    }
}
