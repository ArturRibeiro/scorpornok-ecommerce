using Frameworker.Scorponok.AspNet.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Store.Web.Api.Middlewares;

namespace Store.Web.Api.Extensions
{
    public static class MvcJsonOptionsExtension
    {
        public static IServiceCollection AddNewtonsoftJsonOptions(this IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            return services;
        }

        public static IServiceCollection AddConfigurationMvc(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMvc(options =>
            {
                options.AddNotificationAsyncResultFilter<NotificationAsyncResultFilter>(configuration);
            });

            return services;
        }
    }
}