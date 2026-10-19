using System.Security.Claims;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.History;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Domain.Order.Orders;

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
                new { result.OrderNumber, result.Status, result.Total, Attempt = 1, MaxAttempts = Order.MaxPaymentAttempts }),
            // O preço dos itens vem do Catalog: sem ele, nada é gravado nem cobrado.
            CreateOrderOutcome.Unavailable => Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Catalog unavailable", detail: string.Join(" ", result.Errors)),
            _ => Results.BadRequest(new { result.Errors })
        };
    }

    // Nova tentativa de pagamento de um pedido recusado, com outro cartão (token do POST /card-tokens).
    // 202 com número, status e tentativa; 400 sem cartão; 404 para pedido inexistente ou de outro cliente
    // (não revela que o número existe, como o hub); 409 se o pedido não está recusado ou as tentativas acabaram.
    public static void RetryOrderPayment(this WebApplication app)
        => app.MapPost("/orders/{orderNumber}/payment"
            , ([FromServices] IMemoryBus bus, string orderNumber, PaymentMethod? payment, ClaimsPrincipal user)
                => RetryOrderPaymentAsync(bus, orderNumber, payment, user))
            .RequireAuthorization(CustomerAuthentication.VerifiedCustomerPolicy);

    public static async Task<IResult> RetryOrderPaymentAsync(IMemoryBus bus, string orderNumber, PaymentMethod? payment,
        ClaimsPrincipal user)
    {
        if (CustomerIdentity.From(user) is not { } customer)
            return Results.Forbid();

        var result = await bus.RequestAsync(new RetryOrderPaymentCommand(orderNumber, customer.CustomerId, payment));
        return result.Outcome switch
        {
            RetryOrderPaymentOutcome.Accepted => Results.Accepted($"/orders/{result.OrderNumber}",
                new { result.OrderNumber, result.Status, result.Attempt }),
            RetryOrderPaymentOutcome.NotFound => Results.NotFound(new { result.Errors }),
            RetryOrderPaymentOutcome.Conflict => Results.Conflict(new { result.Errors }),
            _ => Results.BadRequest(new { result.Errors })
        };
    }

    // Histórico do cliente do token: os pedidos dele, do mais recente para o mais antigo, em páginas
    // (pageSize de 1 a 50, padrão 10). 200 com { items, page, pageSize, hasMore }.
    public static void GetMyOrders(this WebApplication app)
        => app.MapGet("/orders"
            , ([FromServices] IMemoryBus bus, ClaimsPrincipal user, int? page, int? pageSize)
                => GetMyOrdersAsync(bus, user, page, pageSize))
            .RequireAuthorization(CustomerAuthentication.VerifiedCustomerPolicy);

    public static async Task<IResult> GetMyOrdersAsync(IMemoryBus bus, ClaimsPrincipal user, int? page, int? pageSize)
    {
        if (CustomerIdentity.From(user) is not { } customer)
            return Results.Forbid();

        return Results.Ok(await bus.RequestAsync(new GetCustomerOrdersQuery(customer.CustomerId, page ?? 1,
            pageSize ?? CustomerOrdersHandler.DefaultPageSize)));
    }

    // Um pedido do histórico, como foi comprado. 404 para pedido inexistente ou de outro cliente, com a
    // mesma mensagem da nova tentativa: não revela que o número existe.
    public static void GetMyOrder(this WebApplication app)
        => app.MapGet("/orders/{orderNumber}"
            , ([FromServices] IMemoryBus bus, string orderNumber, ClaimsPrincipal user)
                => GetMyOrderAsync(bus, orderNumber, user))
            .RequireAuthorization(CustomerAuthentication.VerifiedCustomerPolicy);

    public static async Task<IResult> GetMyOrderAsync(IMemoryBus bus, string orderNumber, ClaimsPrincipal user)
    {
        if (CustomerIdentity.From(user) is not { } customer)
            return Results.Forbid();

        return await bus.RequestAsync(new GetCustomerOrderQuery(customer.CustomerId, orderNumber)) is { } order
            ? Results.Ok(order)
            : Results.NotFound(new { Errors = RetryOrderPaymentResult.NotFound.Errors });
    }
}
