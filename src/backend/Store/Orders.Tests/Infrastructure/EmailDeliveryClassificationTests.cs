namespace Orders.Tests.Infrastructure;

[TestFixture]
public class EmailDeliveryClassificationTests
{
    private static EmailDeliveryFailure Kind(Exception exception) => SmtpOrderEmailSender.Classify(exception).Kind;

    [Test]
    public void Destinatario_recusado_com_5xx_e_falha_do_destinatario()
    {
        Kind(new SmtpCommandException(SmtpErrorCode.RecipientNotAccepted, (SmtpStatusCode)550, "No such user"))
            .Should().Be(EmailDeliveryFailure.Recipient);
    }

    [Test]
    public void Destinatario_recusado_com_4xx_e_temporario()
    {
        Kind(new SmtpCommandException(SmtpErrorCode.RecipientNotAccepted, (SmtpStatusCode)450, "Mailbox busy"))
            .Should().Be(EmailDeliveryFailure.Message);
    }

    [TestCase(SmtpErrorCode.MessageNotAccepted, 554)]
    [TestCase(SmtpErrorCode.UnexpectedStatusCode, 451)]
    public void Recusa_da_mensagem_conta_tentativa(SmtpErrorCode code, int status)
    {
        Kind(new SmtpCommandException(code, (SmtpStatusCode)status, "rejected"))
            .Should().Be(EmailDeliveryFailure.Message);
    }

    [Test]
    public void Remetente_recusado_e_do_servidor_porque_vale_para_todos()
    {
        Kind(new SmtpCommandException(SmtpErrorCode.SenderNotAccepted, (SmtpStatusCode)550, "sender"))
            .Should().Be(EmailDeliveryFailure.Server);
    }

    [Test]
    public void Conexao_TLS_autenticacao_e_protocolo_sao_do_servidor()
    {
        Kind(new SocketException((int)SocketError.ConnectionRefused)).Should().Be(EmailDeliveryFailure.Server);
        Kind(new SslHandshakeException("tls")).Should().Be(EmailDeliveryFailure.Server);
        Kind(new AuthenticationException("535 bad credentials")).Should().Be(EmailDeliveryFailure.Server);
        Kind(new SmtpProtocolException("protocol")).Should().Be(EmailDeliveryFailure.Server);
        Kind(new TimeoutException()).Should().Be(EmailDeliveryFailure.Server);
        Kind(new NotSupportedException("The SMTP server does not support authentication."))
            .Should().Be(EmailDeliveryFailure.Server);
    }

    [Test]
    public void Motivo_guarda_o_codigo_SMTP_e_cabe_na_coluna()
    {
        var result = SmtpOrderEmailSender.Classify(
            new SmtpCommandException(SmtpErrorCode.RecipientNotAccepted, (SmtpStatusCode)550, new string('x', 500)));

        result.Message.Should().StartWith("550 ");
        result.Message.Length.Should().BeLessThanOrEqualTo(250);
    }

    [Test]
    public async Task Endereco_do_pedido_invalido_e_falha_do_destinatario_sem_abrir_conexao()
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@");
        order.ConfirmPayment(Guid.NewGuid(), 1);
        // Host inexistente: se tentasse conectar, a falha seria do servidor.
        var sender = new SmtpOrderEmailSender(Options.Create(new SmtpOptions { Host = "invalido.local", Port = 1 }));

        var act = () => sender.SendPaymentResultAsync(order);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.Kind.Should().Be(EmailDeliveryFailure.Recipient);
    }

    [Test]
    public async Task Remetente_mal_configurado_e_falha_do_servidor()
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@exemplo.com");
        order.ConfirmPayment(Guid.NewGuid(), 1);
        var sender = new SmtpOrderEmailSender(Options.Create(new SmtpOptions { From = "sem-arroba" }));

        var act = () => sender.SendPaymentResultAsync(order);

        (await act.Should().ThrowAsync<EmailDeliveryException>()).Which.Kind.Should().Be(EmailDeliveryFailure.Server);
    }
}
