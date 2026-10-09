namespace Payments.Domain;

public interface IPaymentGateway
{
    Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request);
}