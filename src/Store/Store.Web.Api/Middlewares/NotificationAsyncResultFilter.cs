using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Shared.Code.Notifications;

namespace Store.Web.Api.Middlewares
{
    public class NotificationAsyncResultFilter : IAsyncResultFilter
    {
        //private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly DomainNotificationHandler _domainNotification;
        //private readonly IHostingEnvironment _hostingEnvironment;

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
                    _domainNotification.GetNotifications()
                    .Select(GetMessageNotifications)
                    .ToArray());

                await context.HttpContext.Response.WriteAsync(notifications);

                return;
            }

            next();
        }

        private string GetMessageNotifications(DomainNotification notification)
            => notification.Description;
    }
}
