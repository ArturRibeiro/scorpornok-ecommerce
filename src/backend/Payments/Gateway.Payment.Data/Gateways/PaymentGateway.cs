namespace Gateway.Payment.Data.Gateways;

/// <summary>
/// Gateway simulado: recusa cartões terminados em 0000 e aprova os demais.
/// Serve para exercitar os dois caminhos do fluxo sem um gateway real.
/// </summary>
public class PaymentGateway : IPaymentGateway
{
    public const string RejectedReason = "Card declined by the issuer.";

    public Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request)
    {
        var digits = new string((request.CardNumber ?? "").Where(char.IsDigit).ToArray());
        var result = digits.EndsWith("0000")
            ? new PaymentResult(false, RejectedReason)
            : new PaymentResult(true);
        return Task.FromResult(result);
    }
}
