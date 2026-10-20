namespace Payments.Infrastructure.Gateways;

/// <summary>
/// Gateway simulado: recusa cartões terminados em 0000 e aprova os demais.
/// Serve para exercitar os dois caminhos do fluxo sem um gateway real.
/// Como um gateway real, guarda o resultado por chave de idempotência e o devolve nas
/// chamadas repetidas sem reavaliar. As chaves ficam em memória (registrado como Singleton)
/// e se perdem quando o processo reinicia.
/// </summary>
public class PaymentGateway : IPaymentGateway
{
    public const string RejectedReason = "Card declined by the issuer.";

    private readonly ConcurrentDictionary<string, PaymentResult> _results = new();

    public Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request)
        => Task.FromResult(_results.GetOrAdd(request.IdempotencyKey, _ => Charge(request)));

    // O cartão chega tokenizado: os 4 últimos dígitos bastam para a regra do 0000.
    private static PaymentResult Charge(PaymentRequest request)
        => request.CardLast4 == "0000"
            ? new PaymentResult(false, RejectedReason)
            : new PaymentResult(true);
}
