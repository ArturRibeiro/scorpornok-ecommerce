namespace Orders.Infrastructure.Gateways;

public class PaymentGateway : IPaymentGateway
{
    public async Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request)
    {
        return new PaymentResult(true, "Payment processed successfully", 1);
    }
}