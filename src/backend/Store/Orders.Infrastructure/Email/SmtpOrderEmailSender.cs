namespace Orders.Infrastructure.Email;

/// <summary>
/// Envia o e-mail por SMTP (MailKit). Toda falha sai como <see cref="EmailDeliveryException"/>,
/// classificada em <see cref="Classify"/>, para o dispatcher decidir entre desistir do pedido,
/// tentar de novo mais tarde ou esperar o servidor voltar.
/// </summary>
public class SmtpOrderEmailSender(IOptions<SmtpOptions> options) : IOrderEmailSender
{
    public async Task SendPaymentResultAsync(Order order, CancellationToken cancellationToken = default)
    {
        var smtp = options.Value;
        var email = OrderPaymentEmail.For(order);
        var message = new MimeMessage();
        // Remetente inválido é configuração errada (vale para todos); destinatário inválido é do pedido.
        message.From.Add(ParseAddress(smtp.From, EmailDeliveryFailure.Server, "Remetente (Smtp:From) inválido"));
        message.To.Add(ParseAddress(order.Email, EmailDeliveryFailure.Recipient, "Endereço do pedido inválido"));
        message.Subject = email.Subject;
        message.Body = new TextPart("plain") { Text = email.Body };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(smtp.Host, smtp.Port, smtp.SocketOptions, cancellationToken);
            if (smtp.RequiresAuthentication)
                await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw Classify(ex);
        }
    }

    private static MailboxAddress ParseAddress(string address, EmailDeliveryFailure kind, string reason)
    {
        try
        {
            return MailboxAddress.Parse(address);
        }
        catch (ParseException ex)
        {
            throw new EmailDeliveryException(kind, $"{reason}: {address}", ex);
        }
    }

    /// <summary>Traduz as exceções do MailKit para o tipo de falha (tabela do design de limit-email-retries).</summary>
    public static EmailDeliveryException Classify(Exception exception)
    {
        var kind = exception switch
        {
            EmailDeliveryException delivery => delivery.Kind,
            // Destinatário recusado de forma permanente (5xx): tentar de novo não muda nada.
            SmtpCommandException { ErrorCode: SmtpErrorCode.RecipientNotAccepted } command
                when (int)command.StatusCode >= 500 => EmailDeliveryFailure.Recipient,
            // Remetente recusado é configuração (Smtp:From), igual para todos os pedidos.
            SmtpCommandException { ErrorCode: SmtpErrorCode.SenderNotAccepted } => EmailDeliveryFailure.Server,
            // Recusas temporárias (4xx) e da mensagem contam tentativa.
            SmtpCommandException => EmailDeliveryFailure.Message,
            // Conexão, TLS, autenticação e protocolo: o servidor (ou a configuração) é o problema.
            // NotSupportedException: usuário configurado num servidor que não oferece autenticação.
            SocketException or SslHandshakeException or AuthenticationException or SmtpProtocolException
                or ServiceNotConnectedException or IOException or TimeoutException or OperationCanceledException
                or NotSupportedException
                => EmailDeliveryFailure.Server,
            // Desconhecido: conta tentativa, para um pedido problemático chegar ao limite em vez de travar a fila.
            _ => EmailDeliveryFailure.Message
        };
        return exception as EmailDeliveryException
               ?? new EmailDeliveryException(kind, Describe(exception), exception);
    }

    // Motivo curto para gravar no pedido (coluna limitada) e para o log.
    private static string Describe(Exception exception)
    {
        var text = exception is SmtpCommandException command
            ? $"{(int)command.StatusCode} {command.Message}"
            : $"{exception.GetType().Name}: {exception.Message}";
        return text.Length <= 240 ? text : text[..240];
    }
}
