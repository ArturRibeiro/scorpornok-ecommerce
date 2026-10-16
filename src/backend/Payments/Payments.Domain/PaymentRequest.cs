namespace Payments.Domain;

/// <summary>Pedido de cobrança ao gateway, com o cartão já tokenizado: titular e 4 últimos dígitos.</summary>
/// <param name="IdempotencyKey">Identifica a cobrança (pedido + tentativa); veja <see cref="IPaymentGateway"/>.</param>
public record PaymentRequest(string IdempotencyKey, decimal Amount, string CardHolderName, string CardLast4,
    int Installments);

/// <summary>Resposta do gateway de pagamento.</summary>
/// <param name="Approved">Se a cobrança foi aprovada.</param>
/// <param name="Reason">Motivo da recusa; nulo quando aprovado.</param>
public record PaymentResult(bool Approved, string Reason = null);
