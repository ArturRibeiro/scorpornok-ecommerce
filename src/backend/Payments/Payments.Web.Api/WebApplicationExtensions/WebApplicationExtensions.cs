using Payments.CommandHandlers.CardTokens;
using Shared.Code;

namespace Payments.Web.Api.WebApplicationExtensions;

public static class WebApplicationExtensions
{
    /// <summary>Corpo do POST /card-tokens: o cartão como o cliente digitou no checkout.</summary>
    public record CardTokenRequest(
        string CardHolderName,
        string CardNumber,
        string ExpirationMonth,
        string ExpirationYear,
        string Cvv);

    // Chamado pelo navegador (CORS), como o cofre de um gateway: 201 com o token, 400 com os erros
    // por campo. Sem autenticação: o token sozinho não cobra nada e só vale num pedido autenticado.
    public static void CreateCardToken(this WebApplication app)
        => app.MapPost("/card-tokens", ([FromServices] IMemoryBus bus, CardTokenRequest request)
                => CreateCardTokenAsync(bus, request))
            .WithName("CreateCardToken");

    public static async Task<IResult> CreateCardTokenAsync(IMemoryBus bus, CardTokenRequest request)
    {
        var result = await bus.RequestAsync(new CreateCardTokenCommand(request.CardHolderName, request.CardNumber,
            request.ExpirationMonth, request.ExpirationYear, request.Cvv));
        return result.Created
            ? Results.Created($"/card-tokens/{result.Token}",
                new { token = result.Token, last4 = result.Last4, expiresAt = result.ExpiresAt })
            : Results.BadRequest(new { errors = result.Errors });
    }
}
