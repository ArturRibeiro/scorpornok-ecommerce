using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;

namespace Orders.Web.Api.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    // 201 com número, status e total do pedido; 400 com as mensagens de validação.
    public static void CreateOrder
        (this WebApplication app)
        => app.MapPost("/createOrder"
            , async ([FromServices] IMemoryBus bus , CreateCommand command) =>
            {
                var result = await bus.RequestAsync(command);
                return result.Success
                    ? Results.Created($"/orders/{result.OrderNumber}",
                        new { result.OrderNumber, result.Status, result.Total })
                    : Results.BadRequest(new { result.Errors });
            });
}
