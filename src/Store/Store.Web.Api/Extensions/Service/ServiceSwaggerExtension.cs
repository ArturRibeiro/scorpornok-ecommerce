using Microsoft.Extensions.DependencyInjection;

//using NSwag;

namespace Store.Web.Api.Extensions.Service
{
    public static class ServiceSwaggerExtension
    {
        public static IServiceCollection AddSettingSwaggerDocument(this IServiceCollection @this)
        {
            //@this.AddSwaggerDocument(config =>
            // {
            //     config.PostProcess = document =>
            //     {
            //         document.Info.Version = "v1";
            //         document.Info.Title = "Order";
            //         document.Info.Description = "A simple ASP.NET Core web API";
            //         document.Info.TermsOfService = "None";
            //         document.Info.Contact = new SwaggerContact
            //         {
            //             Name = "Scorponok",
            //             Email = string.Empty,
            //             Url = "https://twitter.com/spboyer"
            //         };
            //         document.Info.License = new SwaggerLicense
            //         {
            //             Name = "Use under LICX",
            //             Url = "https://example.com/license"
            //         };
            //     };
            // });

            return @this;
        }
    }
}
