namespace Payments.CommandHandlers.CardTokens;

/// <summary>
/// Tokeniza o cartão que o navegador enviou. O número completo e o CVV só existem nesta
/// requisição: são validados e descartados, e o token guarda titular, final e validade.
/// </summary>
public record CreateCardTokenCommand(
    string CardHolderName,
    string CardNumber,
    string ExpirationMonth,
    string ExpirationYear,
    string Cvv) : Message<CreateCardTokenResult>
{
    // O record geraria um ToString com o número e o CVV; nada de cartão em log.
    public override string ToString() => nameof(CreateCardTokenCommand);
}

/// <summary>Token criado ou os erros de validação, por campo do cartão.</summary>
public record CreateCardTokenResult(
    string Token,
    string Last4,
    DateTime? ExpiresAt,
    IReadOnlyDictionary<string, string> Errors)
{
    public bool Created => Token is not null;

    public static CreateCardTokenResult From(CardToken token)
        => new(token.Id, token.Last4, token.ExpiresAt, new Dictionary<string, string>());

    public static CreateCardTokenResult Invalid(IReadOnlyDictionary<string, string> errors)
        => new(null, null, null, errors);
}
