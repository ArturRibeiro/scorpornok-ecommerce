using Frameworker.Scorponok.Reading.Database.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Frameworker.Scorponok.Reading.Database
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IServiceCollection AddReadDbScoped(this IServiceCollection service)
        {
            service.AddScoped<IApplicationReadDb, ApplicationReadDb>();
            return service;
        }
    }
}