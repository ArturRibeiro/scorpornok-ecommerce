using Shared.Code.Notifications;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.Web.Api.Filters
{
    public class NotificationAsyncResultFilter : IAsyncResultFilter
    {
        private readonly DomainNotificationHandler _domainNotification;

        public NotificationAsyncResultFilter(INotificationHandler<DomainNotification> notifications)
        {
            _domainNotification = (DomainNotificationHandler)notifications;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (_domainNotification.HasNotifications)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.HttpContext.Response.ContentType = "application/json";

                var notifications = JsonConvert.SerializeObject(
                    _domainNotification.GetNotifications().Select(GetMessageNotifications).ToArray());

                await context.HttpContext.Response.WriteAsync(notifications);

                return;
            }

            await next();
        }

        private string GetMessageNotifications(DomainNotification notification)
            => notification.Description;
    }
}
