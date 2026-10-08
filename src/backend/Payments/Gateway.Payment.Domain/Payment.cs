namespace Gateway.Payment.Domain;

/// <summary>
/// Pagamento processado para um pedido. Guarda só o necessário para auditoria:
/// nada de CVV nem do número completo do cartão, apenas os 4 últimos dígitos.
/// </summary>
public class Payment : Entity<Guid>, IAggregateRoot
{
    public int OrderId { get; private set; }
    public string OrderNumber { get; private set; }
    public decimal Amount { get; private set; }
    public int Installments { get; private set; }
    public string CardHolderName { get; private set; }
    public string CardLast4 { get; private set; }
    public bool Approved { get; private set; }
    /// <summary>Motivo da recusa; nulo quando aprovado.</summary>
    public string Reason { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    // EF
    private Payment() { }

    /// <param name="cardNumber">Número completo, usado só para extrair os 4 últimos dígitos.</param>
    public static Payment Create(int orderId, string orderNumber, decimal amount, int installments,
        string cardHolderName, string cardNumber, PaymentResult result)
    {
        var digits = new string((cardNumber ?? "").Where(char.IsDigit).ToArray());
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            OrderNumber = orderNumber,
            Amount = amount,
            Installments = installments,
            CardHolderName = cardHolderName,
            CardLast4 = digits.Length >= 4 ? digits[^4..] : digits,
            Approved = result.Approved,
            Reason = result.Approved ? null : result.Reason,
            ProcessedAt = DateTime.UtcNow
        };
    }
}
