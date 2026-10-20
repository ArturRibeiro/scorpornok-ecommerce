using AuthorizeAttribute = HotChocolate.Authorization.AuthorizeAttribute;

namespace Customers.Web.Api.GraphQL;

/// <summary>Cadastro como a loja o vê, no GraphQL: os mesmos campos do <c>GET /customers/me</c>.</summary>
public record CustomerProfile(string Email, string? FullName, string? Phone, string? Cpf, CustomerAddress? Address)
{
    public static CustomerProfile From(CommandHandlers.Profiles.CustomerProfile profile)
        => new(profile.Email, profile.FullName, profile.Phone, profile.Cpf,
            profile.Address is { } a
                ? new CustomerAddress(a.Street, a.Number, a.Complement, a.District, a.City, a.State, a.ZipCode)
                : null);
}

public record CustomerAddress(string? Street, string? Number, string? Complement, string? District, string? City,
    string? State, string? ZipCode);

/// <summary>
/// Cadastro inteiro, como no <c>PUT /customers/me</c>. Os campos são opcionais no schema para que um campo
/// faltando chegue à validação do cadastro e volte como erro do campo, e não como erro do GraphQL.
/// </summary>
public record UpdateMyProfileInput(string? FullName, string? Phone, string? Cpf, CustomerAddressInput? Address);

public record CustomerAddressInput(string? Street, string? Number, string? Complement, string? District,
    string? City, string? State, string? ZipCode);

/// <summary>Erro de validação de um campo, nas chaves do JSON de hoje (<c>cpf</c>, <c>address.zipCode</c>).</summary>
public record FieldError(string Field, IReadOnlyList<string> Messages);

/// <summary>O cadastro salvo, ou os erros por campo sem nada salvo (<see cref="Profile"/> nulo).</summary>
public record UpdateMyProfilePayload(CustomerProfile? Profile, IReadOnlyList<FieldError> Errors);

/// <summary>
/// Cadastro do cliente como subgraph do gateway (Fusion). O cliente vem só do token (repassado pelo gateway),
/// com a mesma regra do REST: autenticado, com e-mail confirmado. Sem login → AUTH_NOT_AUTHENTICATED;
/// sem e-mail confirmado → AUTH_NOT_AUTHORIZED.
/// </summary>
[QueryType]
public static partial class CustomerProfileQueries
{
    [Authorize(Policy = CustomerAuthentication.VerifiedCustomerPolicy)]
    public static async Task<CustomerProfile> GetMe(ClaimsPrincipal user, [Service] IMemoryBus bus)
    {
        var customer = CustomerIdentity.From(user)!;
        return CustomerProfile.From(await bus.RequestAsync(new GetCustomerProfileQuery(customer.CustomerId, customer.Email)));
    }
}

[MutationType]
public static partial class CustomerProfileMutations
{
    [Authorize(Policy = CustomerAuthentication.VerifiedCustomerPolicy)]
    public static async Task<UpdateMyProfilePayload> UpdateMyProfile(UpdateMyProfileInput input, ClaimsPrincipal user,
        [Service] IMemoryBus bus)
    {
        var customer = CustomerIdentity.From(user)!;
        var address = input.Address is { } a
            ? new AddressData(a.Street, a.Number, a.Complement, a.District, a.City, a.State, a.ZipCode)
            : null;
        var result = await bus.RequestAsync(new UpdateCustomerProfileCommand(customer.CustomerId, customer.Email,
            input.FullName, input.Phone, input.Cpf, address));

        return result.IsValid
            ? new UpdateMyProfilePayload(CustomerProfile.From(result.Profile!), [])
            : new UpdateMyProfilePayload(null,
                result.Errors.Select(e => new FieldError(e.Key, e.Value)).ToList());
    }
}
