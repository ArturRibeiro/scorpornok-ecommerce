using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Frameworker.Scorponok.AspNet.Mvc
{
    public static class Extensions
    {
        public static MvcOptions AddNotificationAsyncResultFilter<T>(this MvcOptions options, IConfiguration configuration) 
            where T : IAsyncResultFilter
        {
            options.Filters.Add<T>();

            return options;
        }
    }
}
