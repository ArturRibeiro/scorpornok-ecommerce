namespace Orders.Tests.Infrastructure;

// O SmtpOrderEmailSender contra um servidor SMTP de verdade (em memória): conexões, recusas e quedas.
[TestFixture]
public class SmtpOrderEmailSenderTests
{
    private FakeSmtpServer _server = null!;

    [SetUp]
    public void SetUp() => _server = new FakeSmtpServer();

    [TearDown]
    public async Task TearDown() => await _server.DisposeAsync();

    [Test]
    public async Task Servidor_falso_aceita_uma_mensagem_do_MailKit()
    {
        using var client = new SmtpClient();
        await client.ConnectAsync("127.0.0.1", _server.Port, SecureSocketOptions.None);
        await client.AuthenticateAsync("usuario", "senha");
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("loja@scorponok.local"));
        message.To.Add(MailboxAddress.Parse("cliente@exemplo.com"));
        message.Subject = "Pedido confirmado";
        message.Body = new TextPart("plain") { Text = "Seu pedido foi confirmado." };
        await client.SendAsync(message);
        await client.DisconnectAsync(quit: true);

        _server.Connections.Should().Be(1);
        _server.Authentications.Should().Be(1);
        _server.Delivered.Should().Equal("cliente@exemplo.com");
    }

    private SmtpOrderEmailSender Sender(string from = "loja@scorponok.local", int? port = null)
        => new(Options.Create(new SmtpOptions
        {
            Host = "127.0.0.1", Port = port ?? _server.Port, From = from, Username = "usuario", Password = "senha"
        }));

    private static Order ConfirmedOrder(string email)
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail(email);
        order.ConfirmPayment(Guid.NewGuid(), 1);
        return order;
    }

    [Test]
    public async Task Tres_mensagens_saem_por_uma_conexao_e_uma_autenticacao()
    {
        await using (var session = await Sender().OpenSessionAsync())
        {
            foreach (var email in new[] { "a@exemplo.com", "b@exemplo.com", "c@exemplo.com" })
                await session.SendPaymentResultAsync(ConfirmedOrder(email));
        }

        _server.Connections.Should().Be(1);
        _server.Authentications.Should().Be(1);
        _server.Delivered.Should().Equal("a@exemplo.com", "b@exemplo.com", "c@exemplo.com");
    }

    [Test]
    public async Task Destinatario_recusado_e_falha_do_destinatario_e_a_conexao_segue()
    {
        _server.RejectRecipient = address => address == "recusado@exemplo.com";
        await using var session = await Sender().OpenSessionAsync();

        await session.SendPaymentResultAsync(ConfirmedOrder("a@exemplo.com"));
        var refused = () => session.SendPaymentResultAsync(ConfirmedOrder("recusado@exemplo.com"));
        (await refused.Should().ThrowAsync<EmailDeliveryException>()).Which.Kind.Should().Be(EmailDeliveryFailure.Recipient);
        await session.SendPaymentResultAsync(ConfirmedOrder("c@exemplo.com"));

        _server.Connections.Should().Be(1);
        _server.Delivered.Should().Equal("a@exemplo.com", "c@exemplo.com");
    }

    [Test]
    public async Task Conexao_que_cai_no_meio_e_falha_do_servidor()
    {
        _server.DropAfterMessages = 1;
        await using var session = await Sender().OpenSessionAsync();
        await session.SendPaymentResultAsync(ConfirmedOrder("a@exemplo.com"));

        var next = () => session.SendPaymentResultAsync(ConfirmedOrder("b@exemplo.com"));

        (await next.Should().ThrowAsync<EmailDeliveryException>()).Which.Kind.Should().Be(EmailDeliveryFailure.Server);
        _server.Delivered.Should().Equal("a@exemplo.com");
    }

    [Test]
    public async Task Endereco_do_pedido_invalido_e_falha_do_destinatario_sem_enviar_nada()
    {
        await using var session = await Sender().OpenSessionAsync();

        var act = () => session.SendPaymentResultAsync(ConfirmedOrder("cliente@"));

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.Kind.Should().Be(EmailDeliveryFailure.Recipient);
        _server.Delivered.Should().BeEmpty();
    }

    [Test]
    public async Task Servidor_fora_ao_abrir_e_falha_do_servidor()
    {
        // Porta sem ninguém escutando: a do servidor falso, depois de desligado.
        var port = _server.Port;
        await _server.DisposeAsync();
        _server = new FakeSmtpServer();

        var act = () => Sender(port: port).OpenSessionAsync();

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.Kind.Should().Be(EmailDeliveryFailure.Server);
    }

    [Test]
    public async Task Remetente_mal_configurado_e_falha_do_servidor_sem_conectar()
    {
        var act = () => Sender(from: "loja@").OpenSessionAsync();

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.Kind.Should().Be(EmailDeliveryFailure.Server);
        _server.Connections.Should().Be(0);
    }

    [Test]
    public async Task Etapas_aparecem_em_spans_com_o_pedido_e_sem_o_destinatario()
    {
        using var recorder = new OrdersActivityRecorder();
        var first = ConfirmedOrder("maria.souza@exemplo.com");
        var second = ConfirmedOrder("joao.silva@exemplo.com");

        await using (var session = await Sender().OpenSessionAsync())
        {
            await session.SendPaymentResultAsync(first);
            await session.SendPaymentResultAsync(second);
        }

        recorder.Started.Select(a => a.DisplayName)
            .Should().Equal("SMTP conexão", "SMTP autenticação", "SMTP envio", "SMTP envio");
        recorder.Started.Should().OnlyContain(a => a.Duration > TimeSpan.Zero && a.Status != ActivityStatusCode.Error);
        recorder.Started.Where(a => a.DisplayName == "SMTP envio")
            .Select(a => a.GetTagItem(OrderTelemetry.OrderNumberTag))
            .Should().Equal(first.OrderNumber, second.OrderNumber);
        recorder.Started.SelectMany(a => a.TagObjects).Select(tag => tag.Value?.ToString() ?? "")
            .Should().NotContain(value => value.Contains("maria.souza") || value.Contains("joao.silva"));
    }

    [Test]
    public async Task Conexao_recusada_aparece_com_erro_e_o_tipo_da_falha()
    {
        var port = _server.Port;
        await _server.DisposeAsync();
        _server = new FakeSmtpServer();
        using var recorder = new OrdersActivityRecorder();

        var act = () => Sender(port: port).OpenSessionAsync();

        await act.Should().ThrowAsync<EmailDeliveryException>();
        var connect = recorder.Started.Should().ContainSingle().Subject;
        connect.DisplayName.Should().Be("SMTP conexão");
        connect.Status.Should().Be(ActivityStatusCode.Error);
        connect.GetTagItem("email.failure.kind").Should().Be(nameof(EmailDeliveryFailure.Server));
    }

    [Test]
    public async Task Destinatario_recusado_aparece_no_envio_com_erro()
    {
        _server.RejectRecipient = _ => true;
        using var recorder = new OrdersActivityRecorder();
        await using var session = await Sender().OpenSessionAsync();

        var act = () => session.SendPaymentResultAsync(ConfirmedOrder("recusado@exemplo.com"));

        await act.Should().ThrowAsync<EmailDeliveryException>();
        var send = recorder.Started.Single(a => a.DisplayName == "SMTP envio");
        send.Status.Should().Be(ActivityStatusCode.Error);
        send.GetTagItem("email.failure.kind").Should().Be(nameof(EmailDeliveryFailure.Recipient));
    }
}
