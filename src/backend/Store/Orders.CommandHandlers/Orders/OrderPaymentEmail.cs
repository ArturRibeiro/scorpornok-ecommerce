using System.Globalization;

namespace Orders.CommandHandlers.Orders;

/// <summary>Assunto e corpo (texto simples, em português como a loja) do e-mail do resultado do pagamento.</summary>
public record OrderPaymentEmail(string Subject, string Body)
{
    public static OrderPaymentEmail For(Order order)
    {
        var total = FormatReais(order.Total);
        if (order.Status.Code == OrderStatus.NotProcessed.Code)
            return new OrderPaymentEmail(
                $"Pagamento não processado no pedido {order.OrderNumber}",
                $"""
                Não foi possível processar o pagamento do pedido {order.OrderNumber}.

                Tivemos um problema do nosso lado: o pagamento não foi processado e nada foi cobrado.
                Você pode refazer a compra na loja.
                Total: {total}

                Loja Scorponok
                """);
        return order.Status.Code == OrderStatus.Confirmed.Code
            ? new OrderPaymentEmail(
                $"Pedido {order.OrderNumber} confirmado",
                $"""
                Obrigado pela sua compra!

                Seu pagamento foi aprovado e o pedido {order.OrderNumber} está confirmado.
                Total: {total}

                Loja Scorponok
                """)
            : new OrderPaymentEmail(
                $"Pagamento recusado no pedido {order.OrderNumber}",
                $"""
                Não foi possível concluir o pedido {order.OrderNumber}.

                Seu cartão foi recusado, então o pedido não foi confirmado e nada foi cobrado.
                {RetryHint(order)}
                Total: {total}

                Loja Scorponok
                """);
    }

    // Enquanto houver tentativas, o cliente pode pagar o mesmo pedido com outro cartão, na tela da compra.
    private static string RetryHint(Order order)
        => order.CanRetryPayment
            ? "Você pode tentar de novo com outro cartão na tela da compra"
              + $" ({Order.MaxPaymentAttempts - order.Payment.Attempt} tentativa(s) restante(s))."
            : "As tentativas de pagamento deste pedido acabaram; para comprar, faça um novo pedido.";

    // R$ 1.299,99 sem depender da cultura pt-BR, que pode faltar na imagem do container.
    private static string FormatReais(decimal value)
        => "R$ " + value.ToString("#,##0.00", CultureInfo.InvariantCulture)
            .Replace(',', '_').Replace('.', ',').Replace('_', '.');
}
