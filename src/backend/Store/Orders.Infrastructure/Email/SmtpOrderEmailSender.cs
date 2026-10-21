namespace Orders.Infrastructure.Email;

/// <summary>
/// Envia o e-mail por SMTP (MailKit), numa sessão por rodada: conecta e autentica uma vez, envia cada pedido pela
/// mesma conexão e desconecta no descarte. Toda falha sai como <see cref="EmailDeliveryException"/>, classificada
/// em <see cref="Classify"/>, para o dispatcher decidir entre desistir do pedido, tentar de novo mais tarde ou
/// esperar o servidor voltar.
/// </summary>
public class SmtpOrderEmailSender(IOptions<SmtpOptions> options) : IOrderEmailSender
{
    public async Task<IOrderEmailSession> OpenSessionAsync(CancellationToken cancellationToken = default)
    {
        var smtp = options.Value;
        // Remetente inválido é configuração errada (vale para todos): nem conecta.
        var from = ParseAddress(smtp.From, EmailDeliveryFailure.Server, "Remetente (Smtp:From) inválido");
        var client = new SmtpClient();
        try
        {
            using (var connect = StartSmtpActivity("SMTP conexão", smtp))
                await Traced(connect, () => client.ConnectAsync(smtp.Host, smtp.Port, smtp.SocketOptions, cancellationToken));
            if (smtp.RequiresAuthentication)
                using (var authenticate = StartSmtpActivity("SMTP autenticação", smtp))
                    await Traced(authenticate, () => client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken));
            return new Session(client, from);
        }
        catch (Exception ex)
        {
            client.Dispose();
            if (cancellationToken.IsCancellationRequested)
                throw;
            throw Classify(ex);
        }
    }

    // Spans das etapas com o servidor (improve-email-delivery-and-logs): duração e, na falha,
    // o tipo. Nunca o destinatário, o assunto ou o corpo.
    private static Activity? StartSmtpActivity(string name, SmtpOptions smtp)
    {
        var activity = OrdersTelemetry.Source.StartActivity(name, ActivityKind.Client);
        activity?.SetTag("server.address", smtp.Host);
        activity?.SetTag("server.port", smtp.Port);
        return activity;
    }

    private static async Task Traced(Activity? activity, Func<Task> step)
    {
        try
        {
            await step();
        }
        catch (Exception ex)
        {
            var failure = Classify(ex);
            activity?.SetStatus(ActivityStatusCode.Error, failure.Kind.ToString());
            activity?.SetTag("email.failure.kind", failure.Kind.ToString());
            throw;
        }
    }

    private sealed class Session(SmtpClient client, MailboxAddress from) : IOrderEmailSession
    {
        public async Task SendPaymentResultAsync(Order order, CancellationToken cancellationToken = default)
        {
            using var activity = OrdersTelemetry.Source.StartActivity("SMTP envio", ActivityKind.Client);
            OrderTelemetry.Tag(order.OrderNumber);
            await Traced(activity, () => SendAsync(order, cancellationToken));
        }

        private async Task SendAsync(Order order, CancellationToken cancellationToken)
        {
            var email = OrderPaymentEmail.For(order);
            var message = new MimeMessage();
            message.From.Add(from);
            // Destinatário inválido é do pedido: falha sem usar a conexão, que segue para os próximos.
            message.To.Add(ParseAddress(order.Email, EmailDeliveryFailure.Recipient, "Endereço do pedido inválido"));
            message.Subject = email.Subject;
            message.Body = new TextPart("plain") { Text = email.Body };

            try
            {
                // Depois de uma recusa (4xx/5xx), o MailKit reinicia a transação e a conexão continua utilizável;
                // uma conexão que caiu falha aqui como servidor, e o dispatcher encerra a rodada.
                await client.SendAsync(message, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw Classify(ex);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                // QUIT por cortesia: se a conexão já caiu, não há o que encerrar.
                if (client.IsConnected)
                    await client.DisconnectAsync(quit: true);
            }
            catch (Exception ex) when (ex is IOException or SocketException or SmtpProtocolException
                                           or SmtpCommandException or ServiceNotConnectedException) { }
            finally
            {
                client.Dispose();
            }
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
