using MailKit.Security;

namespace Orders.Infrastructure.Email;

/// <summary>
/// Seção "Smtp" da configuração. Com o Mailpit basta Host/Port/From; para um provedor real
/// (ex.: smtp.gmail.com:587) preencha Username/Password e UseStartTls. A senha vem do
/// user-secrets ou da variável Smtp__Password, nunca de um appsettings versionado.
/// </summary>
public class SmtpOptions
{
    public const string Section = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string From { get; set; } = "store@scorponok.local";
    public string Username { get; set; }
    public string Password { get; set; }
    public bool UseStartTls { get; set; }
    /// <summary>Intervalo, em segundos, entre as buscas do OrderEmailDispatcher.</summary>
    public int PollSeconds { get; set; } = 5;

    public SecureSocketOptions SocketOptions => UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
    public bool RequiresAuthentication => !string.IsNullOrWhiteSpace(Username);
}
