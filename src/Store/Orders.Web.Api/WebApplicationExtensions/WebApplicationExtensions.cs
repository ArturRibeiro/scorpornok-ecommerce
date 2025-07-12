namespace Orders.Web.Api.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    public static void CreateOrder
        (this WebApplication app) 
        => app.MapPost("/createOrder"
            , async ([FromServices] IMemoryBus bus , CreateCommand command) 
                => await bus.SendAsync(command))
            .WithOpenApi();
}