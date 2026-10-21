
namespace Orders.Tests.Infrastructure;

/// <summary>
/// Servidor SMTP em memória, só o bastante para o MailKit: conta conexões, autenticações e mensagens, recusa
/// destinatários com 550 (<see cref="RejectRecipient"/>) e derruba a conexão depois de N mensagens
/// (<see cref="DropAfterMessages"/>). Não anuncia PIPELINING, para os comandos chegarem um a um.
/// </summary>
public sealed class FakeSmtpServer : IAsyncDisposable
{
    private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource _stop = new();
    private readonly Task _accepting;
    private int _connections;
    private int _authentications;

    public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;
    public int Connections => _connections;
    public int Authentications => _authentications;

    /// <summary>Destinatários das mensagens aceitas, na ordem.</summary>
    public ConcurrentQueue<string> Delivered { get; } = new();

    public Func<string, bool> RejectRecipient { get; set; } = _ => false;

    /// <summary>Fecha a conexão logo depois de aceitar esta quantidade de mensagens nela.</summary>
    public int? DropAfterMessages { get; set; }

    public FakeSmtpServer()
    {
        _listener.Start();
        _accepting = AcceptAsync();
    }

    private async Task AcceptAsync()
    {
        try
        {
            while (!_stop.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(_stop.Token);
                Interlocked.Increment(ref _connections);
                _ = ServeAsync(client);
            }
        }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
    }

    private async Task ServeAsync(TcpClient client)
    {
        using var _ = client;
        var stream = client.GetStream();
        var reader = new StreamReader(stream, Encoding.ASCII);
        var writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };
        var messages = 0;
        string? recipient = null;
        try
        {
            await writer.WriteLineAsync("220 fake.smtp ESMTP");
            while (await reader.ReadLineAsync(_stop.Token) is { } line)
            {
                var command = line.ToUpperInvariant();
                if (command.StartsWith("EHLO") || command.StartsWith("HELO"))
                {
                    await writer.WriteLineAsync("250-fake.smtp");
                    await writer.WriteLineAsync("250 AUTH PLAIN");
                }
                else if (command.StartsWith("AUTH PLAIN"))
                {
                    // Sem a resposta inicial (SASL-IR), o cliente manda as credenciais na linha seguinte.
                    if (command.Trim() == "AUTH PLAIN")
                    {
                        await writer.WriteLineAsync("334 ");
                        await reader.ReadLineAsync(_stop.Token);
                    }
                    Interlocked.Increment(ref _authentications);
                    await writer.WriteLineAsync("235 2.7.0 Authentication successful");
                }
                else if (command.StartsWith("MAIL FROM"))
                    await writer.WriteLineAsync("250 2.1.0 Ok");
                else if (command.StartsWith("RCPT TO"))
                {
                    var address = line[(line.IndexOf('<') + 1)..line.IndexOf('>')];
                    if (RejectRecipient(address))
                        await writer.WriteLineAsync("550 5.1.1 Recipient rejected");
                    else
                    {
                        recipient = address;
                        await writer.WriteLineAsync("250 2.1.5 Ok");
                    }
                }
                else if (command == "DATA")
                {
                    await writer.WriteLineAsync("354 End data with <CR><LF>.<CR><LF>");
                    while (await reader.ReadLineAsync(_stop.Token) is { } data && data != ".") { }
                    Delivered.Enqueue(recipient ?? "");
                    recipient = null;
                    await writer.WriteLineAsync("250 2.0.0 Ok: queued");
                    if (++messages == DropAfterMessages)
                        return; // derruba a conexão sem QUIT
                }
                else if (command == "RSET" || command == "NOOP")
                {
                    recipient = null;
                    await writer.WriteLineAsync("250 2.0.0 Ok");
                }
                else if (command == "QUIT")
                {
                    await writer.WriteLineAsync("221 2.0.0 Bye");
                    return;
                }
                else
                    await writer.WriteLineAsync("502 5.5.2 Command not recognized");
            }
        }
        catch (Exception ex) when (ex is IOException or OperationCanceledException or ObjectDisposedException) { }
    }

    public async ValueTask DisposeAsync()
    {
        await _stop.CancelAsync();
        _listener.Stop();
        await _accepting;
        _stop.Dispose();
    }
}
