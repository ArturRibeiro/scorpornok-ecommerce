using Catalog.Queries.Products.Queries;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        }

        public static void RegisterQueries(IServiceCollection services)
        {
            services.AddScoped<IProductQueries, ProductQueries>();
        }


    }
}
