namespace Payments.Domain;

/// <summary>Situação do pagamento: gravado antes de cobrar e completado com a resposta do gateway.</summary>
public enum PaymentStatus
{
    Processing,
    Approved,
    Rejected,
    /// <summary>Anulado sem cobrança: a consulta do Orders chegou antes de qualquer solicitação.</summary>
    Voided
}

/// <summary>
/// Pagamento de uma tentativa de um pedido. Guarda só o necessário para auditoria:
/// nada de CVV nem do número completo do cartão, apenas os 4 últimos dígitos.
/// </summary>
public class Payment : Entity<Guid>, IAggregateRoot
{
    public int OrderId { get; private set; }
    public string OrderNumber { get; private set; }
    /// <summary>Tentativa de pagamento do pedido (1 até haver nova tentativa com outro cartão).</summary>
    public int Attempt { get; private set; }
    /// <summary>Identifica a cobrança no gateway: a mesma chave nunca gera duas cobranças.</summary>
    public string IdempotencyKey { get; private set; }
    public decimal Amount { get; private set; }
    public int Installments { get; private set; }
    public string CardHolderName { get; private set; }
    public string CardLast4 { get; private set; }
    public PaymentStatus Status { get; private set; }
    public bool Approved => Status == PaymentStatus.Approved;
    /// <summary>Motivo da recusa; nulo quando aprovado ou em processamento.</summary>
    public string Reason { get; private set; }
    public DateTime RequestedAt { get; private set; }
    /// <summary>Quando o resultado do gateway foi registrado; nulo em processamento.</summary>
    public DateTime? ProcessedAt { get; private set; }

    // EF
    private Payment() { }

    public static string KeyFor(string orderNumber, int attempt) => $"{orderNumber}-{attempt}";

    /// <summary>Registra o pagamento em processamento, antes de cobrar.</summary>
    /// <param name="cardNumber">Número completo, usado só para extrair os 4 últimos dígitos.</param>
    public static Payment Start(int orderId, string orderNumber, int attempt, decimal amount, int installments,
        string cardHolderName, string cardNumber)
    {
        var digits = new string((cardNumber ?? "").Where(char.IsDigit).ToArray());
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            OrderNumber = orderNumber,
            Attempt = attempt,
            IdempotencyKey = KeyFor(orderNumber, attempt),
            Amount = amount,
            Installments = installments,
            CardHolderName = cardHolderName,
            CardLast4 = digits.Length >= 4 ? digits[^4..] : digits,
            Status = PaymentStatus.Processing,
            RequestedAt = DateTime.UtcNow
        };
    }

    public const string VoidedReason = "Pedido anulado: o pagamento não foi processado a tempo.";

    /// <summary>
    /// Anula a tentativa que ainda não tem pagamento, ocupando a mesma chave da cobrança: uma
    /// solicitação que chegue depois encontra o registro e é recusada sem passar pelo gateway.
    /// Não há dados do cartão nem valor, porque nenhuma solicitação chegou.
    /// </summary>
    public static Payment Void(string orderNumber, int attempt)
    {
        var now = DateTime.UtcNow;
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Attempt = attempt,
            IdempotencyKey = KeyFor(orderNumber, attempt),
            CardHolderName = "",
            CardLast4 = "",
            Status = PaymentStatus.Voided,
            Reason = VoidedReason,
            RequestedAt = now,
            ProcessedAt = now
        };
    }

    /// <summary>Registra a resposta do gateway; só age sobre um pagamento em processamento.</summary>
    public bool Complete(PaymentResult result)
    {
        if (Status != PaymentStatus.Processing) return false;
        Status = result.Approved ? PaymentStatus.Approved : PaymentStatus.Rejected;
        Reason = result.Approved ? null : result.Reason;
        ProcessedAt = DateTime.UtcNow;
        return true;
    }
}
