using System.Globalization;

namespace Orders.CommandHandlers.Orders;

/// <summary>Assunto e corpo (texto simples, em inglês como a loja) do e-mail do resultado do pagamento.</summary>
public record OrderPaymentEmail(string Subject, string Body)
{
    public static OrderPaymentEmail For(Order order)
    {
        var total = order.Total.ToString("0.00", CultureInfo.InvariantCulture);
        return order.Status.Code == OrderStatus.Confirmed.Code
            ? new OrderPaymentEmail(
                $"Order {order.OrderNumber} confirmed",
                $"""
                Thank you for your order!

                Your payment was approved and order {order.OrderNumber} is confirmed.
                Total: ${total}

                Scorponok Store
                """)
            : new OrderPaymentEmail(
                $"Payment declined for order {order.OrderNumber}",
                $"""
                We couldn't complete order {order.OrderNumber}.

                Your card was declined, so the order was not confirmed and nothing was charged.
                Total: ${total}

                Scorponok Store
                """);
    }
}
