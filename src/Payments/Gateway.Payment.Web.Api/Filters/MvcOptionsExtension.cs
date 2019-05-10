using Gateway.Payment.Web.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gateway.Payment.Web.Api.Filters
{
    public static class MvcOptionsExtension
    {
        public static MvcOptions ConfigureFilters(this MvcOptions options, IConfiguration configuration)
        {
            options.Filters.Add<NotificationAsyncResultFilter>();

            return options;
        }
    }
}
