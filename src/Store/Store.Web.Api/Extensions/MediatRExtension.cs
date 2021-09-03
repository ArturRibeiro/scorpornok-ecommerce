using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Store.Web.Api.Extensions
{
    public static class MediatRExtension
    {
        public static IServiceCollection AddConfigurationMediatR(this IServiceCollection services)
        {
            services
                .AddMediatR(typeof(IRequestHandler<,>))
                .AddMediatR(typeof(INotificationHandler<>));
            return services;
        }
    }
}