using Catalog.Queries.Products;
using Frameworker.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

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
            .WithName("GetAllProducts");
    }

    public static void GetProductById(this WebApplication app)
    {
        app.MapGet("/GetProductById/{id:long}", async Task<Results<Ok<ProductItemMessageResponse>, NotFound>> ([FromServices] IProductQueries queries, long id) =>
            {
                var product = await queries.GetProductById(id);
                return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
            })
            .WithName("GetProductById");
    }
}
