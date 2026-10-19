using System.Security.Claims;
using HotChocolate.Authorization;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.History;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Domain.Order.Orders;

namespace Orders.Web.Api.GraphQL;

/// <summary>Códigos dos erros do pedido, um por desfecho de hoje; a loja decide a tela pelo código.</summary>
public static class OrderErrorCodes
{
    public const string Validation = "VALIDATION";
    public const string CatalogUnavailable = "CATALOG_UNAVAILABLE";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
}

/// <summary>Erro do pedido: o código do desfecho e a mensagem de hoje (ex.: "CPF inválido.").</summary>
public record UserError(string Code, string Message);

// Entradas no formato do POST /createOrder. Os campos são opcionais no schema para que a falta de um
// chegue à validação do pedido e volte como mensagem, e não como erro do GraphQL. Preço, nome e foto dos
// itens vêm do Catalog; o cliente e o e-mail, do token.
public record PlaceOrderInput(OrderAddressInput? Address, OrderBuyerInput? Customer, IReadOnlyList<OrderItemInput>? Items,
    PaymentInput? Payment);

public record OrderAddressInput(string? Street, string? Number, string? Complement, string? District, string? City,
    string? State, string? ZipCode);

public record OrderBuyerInput(string? FullName, string? Phone, string? Cpf);

public record OrderItemInput(int ProductId, int Units);

/// <summary>Só o token do cartão (POST /card-tokens do Payments); o cartão nunca chega ao Orders.</summary>
public record PaymentInput(string? CardToken, int Installments = 1);

public record PlacedOrder(string OrderNumber, string Status, decimal Total, int Attempt, int MaxAttempts);

/// <summary>O pedido registrado, ou os erros sem nada registrado (<see cref="Order"/> nulo).</summary>
public record PlaceOrderPayload(PlacedOrder? Order, IReadOnlyList<UserError> Errors);

public record RetryOrderPaymentInput(string OrderNumber, string? CardToken, int Installments = 1);

/// <summary>A nova tentativa aceita (pedido de volta a pendente), ou os erros sem nada alterado.</summary>
public record RetryOrderPaymentPayload(string? OrderNumber, string? Status, int? Attempt, IReadOnlyList<UserError> Errors);

/// <summary>
/// Pedidos do cliente como subgraph do gateway (Fusion), sobre os mesmos handlers dos endpoints REST. O
/// cliente vem só do token (repassado pelo gateway), com a política VerifiedCustomer: sem login →
/// AUTH_NOT_AUTHENTICATED; sem e-mail confirmado → AUTH_NOT_AUTHORIZED. O acompanhamento do pagamento
/// continua no hub SignalR.
/// </summary>
[QueryType]
public static partial class OrderQueries
{
    [Authorize(Policy = CustomerAuthentication.VerifiedCustomerPolicy)]
    public static async Task<CustomerOrderPage> GetMyOrders(int? page, int? pageSize, ClaimsPrincipal user,
        [Service] IMemoryBus bus)
        => await bus.RequestAsync(new GetCustomerOrdersQuery(CustomerIdentity.From(user)!.CustomerId, page ?? 1,
            pageSize ?? CustomerOrdersHandler.DefaultPageSize));

    // Nulo para pedido inexistente ou de outro cliente: não revela que o número existe.
    [Authorize(Policy = CustomerAuthentication.VerifiedCustomerPolicy)]
    public static async Task<CustomerOrderDetail?> GetMyOrder(string orderNumber, ClaimsPrincipal user,
        [Service] IMemoryBus bus)
        => await bus.RequestAsync(new GetCustomerOrderQuery(CustomerIdentity.From(user)!.CustomerId, orderNumber));
}

[MutationType]
public static partial class OrderMutations
{
    [Authorize(Policy = CustomerAuthentication.VerifiedCustomerPolicy)]
    public static async Task<PlaceOrderPayload> PlaceOrder(PlaceOrderInput input, ClaimsPrincipal user,
        [Service] IMemoryBus bus)
    {
        var customer = CustomerIdentity.From(user)!;
        var address = input.Address is { } a
            ? new OrderAddressMessageResponse
            {
                Street = a.Street, Number = a.Number, Complement = a.Complement, District = a.District, City = a.City,
                State = a.State, ZipCode = a.ZipCode
            }
            // Sem endereço, um vazio: a falta vira as mensagens de validação do pedido.
            : new OrderAddressMessageResponse();
        var items = (input.Items ?? []).Select(i => new OrderItemMessageResponse { ProductId = i.ProductId, Units = i.Units })
            .ToList();
        var buyer = input.Customer is { } b ? new OrderBuyerMessage(b.FullName, b.Phone, b.Cpf) : null;
        var payment = input.Payment is { } p ? new PaymentMethod(p.CardToken, p.Installments) : null;

        var result = await bus.RequestAsync(new CreateCommand(customer.CustomerId, address, items, payment,
            customer.Email, buyer));
        return result.Outcome switch
        {
            CreateOrderOutcome.Created => new PlaceOrderPayload(
                new PlacedOrder(result.OrderNumber!, result.Status!, result.Total, 1, Order.MaxPaymentAttempts), []),
            CreateOrderOutcome.Unavailable => new PlaceOrderPayload(null, Errors(OrderErrorCodes.CatalogUnavailable, result.Errors)),
            _ => new PlaceOrderPayload(null, Errors(OrderErrorCodes.Validation, result.Errors))
        };
    }

    [Authorize(Policy = CustomerAuthentication.VerifiedCustomerPolicy)]
    public static async Task<RetryOrderPaymentPayload> RetryOrderPayment(RetryOrderPaymentInput input,
        ClaimsPrincipal user, [Service] IMemoryBus bus)
    {
        var customer = CustomerIdentity.From(user)!;
        var result = await bus.RequestAsync(new RetryOrderPaymentCommand(input.OrderNumber, customer.CustomerId,
            new PaymentMethod(input.CardToken, input.Installments)));
        return result.Outcome switch
        {
            RetryOrderPaymentOutcome.Accepted => new RetryOrderPaymentPayload(result.OrderNumber, result.Status,
                result.Attempt, []),
            RetryOrderPaymentOutcome.NotFound => Failed(OrderErrorCodes.NotFound, result.Errors),
            RetryOrderPaymentOutcome.Conflict => Failed(OrderErrorCodes.Conflict, result.Errors),
            _ => Failed(OrderErrorCodes.Validation, result.Errors)
        };
    }

    private static RetryOrderPaymentPayload Failed(string code, IReadOnlyCollection<string> messages)
        => new(null, null, null, Errors(code, messages));

    private static IReadOnlyList<UserError> Errors(string code, IReadOnlyCollection<string> messages)
        => messages.Select(m => new UserError(code, m)).ToList();
}
