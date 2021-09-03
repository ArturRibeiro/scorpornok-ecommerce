using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Store.Web.Api.Extensions
{
    public static class SwaggerExtension
    {
        public static IServiceCollection AddConfigurationSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Store.Web.Api", Version = "v1" });
            });
            return services;
        }
    }
}