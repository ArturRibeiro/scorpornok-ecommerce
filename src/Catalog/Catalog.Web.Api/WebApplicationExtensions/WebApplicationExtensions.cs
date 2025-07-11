using Catalog.Queries.Products;
using Frameworker.EntityFrameworkCore;

namespace Catalog.Web.Api.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    public static void GetAllProducts(this WebApplication app)
    {
        app.MapGet("/GetAllProducts", async ([FromServices] IProductQueries queries, [AsParameters] PagingModel paging) =>
            {
                IPagedList<ProductItemMessageResponse> products = await queries.GetAllProducts(paging);
                return products;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();
    }
}