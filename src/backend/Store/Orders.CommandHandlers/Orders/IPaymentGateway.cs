namespace Orders.CommandHandlers.Orders;

public interface IPaymentGateway
{
    Task<PaymentResult> SendProcessPaymentAsync(PaymentRequest request);
}