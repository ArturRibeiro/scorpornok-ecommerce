namespace Gateway.Payment.Domain;

public interface IPaymentGateway
{
    Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request);
}