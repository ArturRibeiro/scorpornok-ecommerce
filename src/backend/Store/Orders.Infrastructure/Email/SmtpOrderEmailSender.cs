using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Orders.Infrastructure.Email;

public class SmtpOrderEmailSender(IOptions<SmtpOptions> options) : IOrderEmailSender
{
    public async Task SendPaymentResultAsync(Order order, CancellationToken cancellationToken = default)
    {
        var smtp = options.Value;
        var email = OrderPaymentEmail.For(order);
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(smtp.From));
        message.To.Add(MailboxAddress.Parse(order.Email));
        message.Subject = email.Subject;
        message.Body = new TextPart("plain") { Text = email.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp.Host, smtp.Port, smtp.SocketOptions, cancellationToken);
        if (smtp.RequiresAuthentication)
            await client.AuthenticateAsync(smtp.Username, smtp.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
