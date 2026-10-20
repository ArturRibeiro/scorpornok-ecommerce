namespace Customers.Web.Api.WebApplicationExtensions;

/// <summary>Corpo do PUT /customers/me: o cadastro inteiro (o e-mail vem do token).</summary>
public record CustomerProfileRequest(string? FullName, string? Phone, string? Cpf, AddressData? Address);

public static class WebApplicationExtensions
{
    // 200 com o cadastro; sem cadastro salvo, 200 só com o e-mail do token.
    // Sem token → 401 e sem e-mail confirmado → 403, pela política.
    public static void GetCustomerProfile(this WebApplication app)
        => app.MapGet("/customers/me", ([FromServices] IMemoryBus bus, ClaimsPrincipal user) => GetProfileAsync(bus, user))
            .RequireAuthorization(CustomerAuthentication.VerifiedCustomerPolicy);

    // 200 com o cadastro salvo; 400 com os erros por campo ({ errors: { "cpf": [...] } }), sem gravar.
    public static void UpdateCustomerProfile(this WebApplication app)
        => app.MapPut("/customers/me", ([FromServices] IMemoryBus bus, CustomerProfileRequest request, ClaimsPrincipal user)
                => UpdateProfileAsync(bus, request, user))
            .RequireAuthorization(CustomerAuthentication.VerifiedCustomerPolicy);

    // O cliente vem só do token: não há como ler ou alterar o cadastro de outra conta.
    public static async Task<IResult> GetProfileAsync(IMemoryBus bus, ClaimsPrincipal user)
    {
        if (CustomerIdentity.From(user) is not { } customer)
            return Results.Forbid();

        return Results.Ok(await bus.RequestAsync(new GetCustomerProfileQuery(customer.CustomerId, customer.Email)));
    }

    public static async Task<IResult> UpdateProfileAsync(IMemoryBus bus, CustomerProfileRequest request, ClaimsPrincipal user)
    {
        if (CustomerIdentity.From(user) is not { } customer)
            return Results.Forbid();

        var result = await bus.RequestAsync(new UpdateCustomerProfileCommand(customer.CustomerId, customer.Email,
            request.FullName, request.Phone, request.Cpf, request.Address));
        return result.IsValid ? Results.Ok(result.Profile) : Results.BadRequest(new { result.Errors });
    }
}
