using System.Security.Claims;

namespace Orders.Web.Api.Authentication;

/// <summary>
/// Cliente do pedido, lido dos claims do token do Keycloak: <c>sub</c> (GUID da conta)
/// e <c>email</c>. É a única fonte do cliente e do e-mail do pedido; o corpo é ignorado.
/// </summary>
public record CustomerIdentity(Guid CustomerId, string Email)
{
    /// <summary>Nulo quando falta o <c>sub</c> (ou não é GUID) ou o <c>email</c>.</summary>
    public static CustomerIdentity? From(ClaimsPrincipal user)
    {
        var email = user.FindFirst("email")?.Value;
        return Guid.TryParse(user.FindFirst("sub")?.Value, out var customerId) && !string.IsNullOrWhiteSpace(email)
            ? new CustomerIdentity(customerId, email)
            : null;
    }
}
