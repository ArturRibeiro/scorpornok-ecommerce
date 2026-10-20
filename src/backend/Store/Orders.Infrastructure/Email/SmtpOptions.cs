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
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseStartTls { get; set; }
    /// <summary>Intervalo, em segundos, entre as buscas do OrderEmailDispatcher.</summary>
    public int PollSeconds { get; set; } = 5;
    /// <summary>Falhas temporárias de um pedido até o envio ser encerrado como falha definitiva.</summary>
    public int MaxEmailAttempts { get; set; } = 6;
    /// <summary>Espera, em segundos, depois da 1ª falha temporária; dobra a cada falha seguinte.</summary>
    public int RetryBaseSeconds { get; set; } = 60;
    /// <summary>Teto, em segundos, da espera entre tentativas de um pedido e entre rodadas com o servidor fora.</summary>
    public int RetryMaxSeconds { get; set; } = 3600;

    public bool IsValid => PollSeconds > 0 && MaxEmailAttempts > 0 && RetryBaseSeconds > 0 && RetryMaxSeconds > 0;

    /// <summary>Espera antes da tentativa seguinte à falha número <paramref name="attempt"/> (1, 2, ...).</summary>
    public TimeSpan RetryDelay(int attempt)
        => TimeSpan.FromSeconds(Math.Min(RetryBaseSeconds * Math.Pow(2, Math.Max(0, attempt - 1)), RetryMaxSeconds));

    public SecureSocketOptions SocketOptions => UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
    public bool RequiresAuthentication => !string.IsNullOrWhiteSpace(Username);
}
