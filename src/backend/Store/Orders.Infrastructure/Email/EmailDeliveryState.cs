namespace Orders.Infrastructure.Email;

/// <summary>
/// Se a última rodada do <see cref="OrderEmailDispatcher"/> parou com o servidor de e-mail fora.
/// Fica em memória (singleton): depois de um reinício, o primeiro envio recalcula.
/// </summary>
public class EmailDeliveryState
{
    private volatile bool _serverUnavailable;

    public bool ServerUnavailable => _serverUnavailable;
    public string? LastServerError { get; private set; }

    public void MarkServerUnavailable(string reason)
    {
        LastServerError = reason;
        _serverUnavailable = true;
    }

    public void MarkServerAvailable() => _serverUnavailable = false;
}
