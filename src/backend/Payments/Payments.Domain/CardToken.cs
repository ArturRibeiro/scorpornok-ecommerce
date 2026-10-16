namespace Payments.Domain;

/// <summary>
/// Token do cartão, como o cofre de um gateway real: o navegador envia o cartão ao Payments e
/// recebe este token, e só ele trafega pelo Orders e pelas filas. Guarda o que a cobrança simulada
/// precisa (titular, 4 últimos dígitos e validade); nunca o número completo nem o CVV.
/// Vale 24 h (cobre o broker fora com o pedido no outbox) e serve para uma única cobrança.
/// </summary>
public class CardToken
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public string Id { get; private set; }
    public string CardHolderName { get; private set; }
    public string Last4 { get; private set; }
    public string ExpirationMonth { get; private set; }
    public string ExpirationYear { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    /// <summary>Quando o token foi usado numa cobrança; nulo enquanto está livre.</summary>
    public DateTime? UsedAt { get; private set; }

    // EF
    private CardToken() { }

    /// <param name="cardNumber">Número completo, usado só para extrair os 4 últimos dígitos.</param>
    public static CardToken Create(string cardHolderName, string cardNumber, string expirationMonth,
        string expirationYear, DateTime now)
    {
        var digits = new string((cardNumber ?? "").Where(char.IsDigit).ToArray());
        return new CardToken
        {
            Id = "tok_" + Guid.NewGuid().ToString("N"),
            CardHolderName = cardHolderName.Trim(),
            Last4 = digits.Length >= 4 ? digits[^4..] : digits,
            ExpirationMonth = expirationMonth.PadLeft(2, '0'),
            ExpirationYear = expirationYear,
            CreatedAt = now,
            ExpiresAt = now + Lifetime
        };
    }

    /// <summary>Livre e dentro da validade.</summary>
    public bool CanBeUsed(DateTime now) => UsedAt is null && ExpiresAt > now;

    public void MarkUsed(DateTime now) => UsedAt = now;
}

public interface ICardTokenRepository
{
    Task<CardToken> GetAsync(string id, CancellationToken cancellationToken = default);
    void Add(CardToken token);
    IUnitOfWork UnitOfWork { get; }
}
