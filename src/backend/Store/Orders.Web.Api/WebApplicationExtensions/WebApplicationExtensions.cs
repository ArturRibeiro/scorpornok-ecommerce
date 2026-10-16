using System.Security.Claims;

namespace Orders.Web.Api.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    // 201 com número, status e total do pedido; 400 com as mensagens de validação.
    // Sem token → 401 e sem e-mail confirmado → 403, pela política.
    public static void CreateOrder
        (this WebApplication app)
        => app.MapPost("/createOrder"
            , ([FromServices] IMemoryBus bus, CreateCommand command, ClaimsPrincipal user)
                => CreateOrderAsync(bus, command, user))
            .RequireAuthorization(CustomerAuthentication.VerifiedCustomerPolicy);

    // O cliente e o e-mail do pedido vêm do token; o que vier no corpo é sobrescrito.
    public static async Task<IResult> CreateOrderAsync(IMemoryBus bus, CreateCommand command, ClaimsPrincipal user)
    {
        if (CustomerIdentity.From(user) is not { } customer)
            return Results.Forbid();

        var result = await bus.RequestAsync(command with { UserId = customer.CustomerId, Email = customer.Email });
        return result.Outcome switch
        {
            CreateOrderOutcome.Created => Results.Created($"/orders/{result.OrderNumber}",
                new { result.OrderNumber, result.Status, result.Total }),
            // O preço dos itens vem do Catalog: sem ele, nada é gravado nem cobrado.
            CreateOrderOutcome.Unavailable => Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Catalog unavailable", detail: string.Join(" ", result.Errors)),
            _ => Results.BadRequest(new { result.Errors })
        };
    }
}
