namespace Payments.Domain;

/// <summary>
/// Gateway de pagamento. Contrato de idempotência, como nos gateways reais: a mesma
/// <see cref="PaymentRequest.IdempotencyKey"/> devolve sempre a resposta da primeira chamada,
/// sem cobrar o cartão de novo. Chaves diferentes são cobranças independentes.
/// </summary>
public interface IPaymentGateway
{
    Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request);
}
