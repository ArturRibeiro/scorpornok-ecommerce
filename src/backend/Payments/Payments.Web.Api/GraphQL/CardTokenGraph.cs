using Payments.CommandHandlers.CardTokens;
using Shared.Code;

namespace Payments.Web.Api.GraphQL;

/// <summary>O cartão como o cliente digitou no checkout, como no <c>POST /card-tokens</c>.</summary>
public record TokenizeCardInput(string CardHolderName, string CardNumber, string ExpirationMonth,
    string ExpirationYear, string Cvv)
{
    // Nunca mostra o cartão (ex.: num log que serialize o input).
    public override string ToString() => nameof(TokenizeCardInput);
}

/// <summary>O token criado: só ele segue para o pedido. Guarda os 4 últimos dígitos, nunca o número nem o CVV.</summary>
public record CardTokenCreated(string Token, string Last4, DateTime? ExpiresAt);

/// <summary>Erro de um campo do cartão, com o nome do campo do formulário (<c>cardNumber</c>, <c>cvv</c>, ...).</summary>
public record CardFieldError(string Field, string Message);

/// <summary>O token, ou os erros por campo sem nada criado (<see cref="Token"/> nulo).</summary>
public record TokenizeCardPayload(CardTokenCreated? Token, IReadOnlyList<CardFieldError> Errors);

/// <summary>
/// Tokenização do cartão como subgraph do gateway (Fusion), sobre o mesmo handler do REST. Sem login, como o
/// REST: o token sozinho não cobra nada e só vale num pedido autenticado. O cartão passa pelo gateway, que não
/// o registra (veja o design da change add-payments-subgraph).
/// </summary>
[MutationType]
public static partial class CardTokenMutations
{
    public static async Task<TokenizeCardPayload> TokenizeCard(TokenizeCardInput input, [Service] IMemoryBus bus)
    {
        var result = await bus.RequestAsync(new CreateCardTokenCommand(input.CardHolderName, input.CardNumber,
            input.ExpirationMonth, input.ExpirationYear, input.Cvv));
        return result.Created
            ? new TokenizeCardPayload(new CardTokenCreated(result.Token, result.Last4, result.ExpiresAt), [])
            : new TokenizeCardPayload(null, result.Errors.Select(e => new CardFieldError(e.Key, e.Value)).ToList());
    }
}
