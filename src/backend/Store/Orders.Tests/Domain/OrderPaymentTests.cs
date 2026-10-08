namespace Orders.Tests.Domain;

[TestFixture]
public class OrderPaymentTests
{
    private static Order PendingOrder() => new(Guid.NewGuid());

    [Test]
    public void ConfirmPayment_em_pedido_pendente_deve_confirmar_e_guardar_o_PaymentId()
    {
        var order = PendingOrder();
        var paymentId = Guid.NewGuid();

        order.ConfirmPayment(paymentId).Should().BeTrue();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.PaymentId.Should().Be(paymentId);
    }

    [Test]
    public void FailPayment_em_pedido_pendente_deve_marcar_falha_sem_PaymentId()
    {
        var order = PendingOrder();

        order.FailPayment().Should().BeTrue();

        order.Status.Should().Be(OrderStatus.Failed);
        order.PaymentId.Should().BeNull();
    }

    [Test]
    public void Pedido_confirmado_nao_deve_mudar_com_novo_resultado()
    {
        var order = PendingOrder();
        var paymentId = Guid.NewGuid();
        order.ConfirmPayment(paymentId);

        order.ConfirmPayment(Guid.NewGuid()).Should().BeFalse();
        order.FailPayment().Should().BeFalse();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.PaymentId.Should().Be(paymentId);
    }

    [Test]
    public void Pedido_com_falha_nao_deve_mudar_com_novo_resultado()
    {
        var order = PendingOrder();
        order.FailPayment();

        order.ConfirmPayment(Guid.NewGuid()).Should().BeFalse();

        order.Status.Should().Be(OrderStatus.Failed);
        order.PaymentId.Should().BeNull();
    }

    [Test]
    public void Pedido_pendente_nao_deve_aguardar_email()
        => PendingOrder().IsAwaitingPaymentEmail.Should().BeFalse();

    [Test]
    public void Pedido_confirmado_ou_recusado_deve_aguardar_email_ate_ser_marcado()
    {
        var confirmed = PendingOrder();
        confirmed.ConfirmPayment(Guid.NewGuid());
        var failed = PendingOrder();
        failed.FailPayment();

        confirmed.IsAwaitingPaymentEmail.Should().BeTrue();
        failed.IsAwaitingPaymentEmail.Should().BeTrue();

        confirmed.MarkPaymentEmailSent();

        confirmed.IsAwaitingPaymentEmail.Should().BeFalse();
        confirmed.PaymentEmailSentAt.Should().NotBeNull();
    }

    [Test]
    public void Segunda_marcacao_deve_manter_a_data_do_primeiro_envio()
    {
        var order = PendingOrder();
        order.ConfirmPayment(Guid.NewGuid());
        order.MarkPaymentEmailSent();
        var first = order.PaymentEmailSentAt;

        order.MarkPaymentEmailSent();

        order.PaymentEmailSentAt.Should().Be(first);
    }
}
