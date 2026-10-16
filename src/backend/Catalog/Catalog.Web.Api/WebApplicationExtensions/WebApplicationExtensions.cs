namespace Catalog.Web.Api.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    public static void GetAllProducts(this WebApplication app)
    {
        app.MapGet("/Products", async ([FromServices] IProductQueries queries, [AsParameters] PagingModel paging) =>
            {
                IPagedList<ProductItemMessageResponse> products = await queries.GetAllProducts(paging);
                return products;
            })
            .WithName("Products");
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

    public const int MaxProductsByIds = 100;

    // Vários produtos de uma vez (ex.: os itens de um pedido); os ids inexistentes não aparecem.
    public static void GetProductsByIds(this WebApplication app)
    {
        app.MapGet("/GetProductsByIds", async Task<Results<Ok<IReadOnlyList<ProductItemMessageResponse>>, BadRequest<string>>>
                ([FromServices] IProductQueries queries, [FromQuery] long[]? ids) =>
            {
                if (ids is not { Length: > 0 })
                    return TypedResults.BadRequest("Informe ao menos um id.");
                if (ids.Length > MaxProductsByIds)
                    return TypedResults.BadRequest($"Informe no máximo {MaxProductsByIds} ids.");
                return TypedResults.Ok(await queries.GetProductsByIds(ids.Distinct().ToArray()));
            })
            .WithName("GetProductsByIds");
    }
}
