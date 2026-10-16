namespace Orders.CommandHandlers.Orders;

/// <summary>De quem é a falha no envio do e-mail; decide o que o dispatcher faz com o pedido.</summary>
public enum EmailDeliveryFailure
{
    /// <summary>Endereço do pedido inválido ou recusado de forma permanente: não adianta tentar de novo.</summary>
    Recipient,
    /// <summary>Recusa temporária da mensagem: conta tentativa e tenta de novo mais tarde.</summary>
    Message,
    /// <summary>Servidor fora, conexão segura ou autenticação: vale para todos os pedidos, não conta tentativa.</summary>
    Server
}

/// <summary>Lançada pelo <see cref="IOrderEmailSender"/> quando o e-mail não pôde ser entregue.</summary>
public class EmailDeliveryException(EmailDeliveryFailure kind, string reason, Exception? innerException = null)
    : Exception(reason, innerException)
{
    public EmailDeliveryFailure Kind { get; } = kind;
}
