namespace Orders.Tests.Domain;

[TestFixture]
public class OrderPaymentTests
{
    private static Order PendingOrder() => new(Guid.NewGuid());

    [Test]
    public void Pedido_novo_nasce_com_o_pagamento_vazio()
    {
        var order = PendingOrder();

        order.Payment.Should().NotBeNull();
        order.Payment.PaymentId.Should().BeNull();
        order.Payment.EmailSentAt.Should().BeNull();
    }

    [Test]
    public void ConfirmPayment_em_pedido_pendente_deve_confirmar_e_guardar_o_PaymentId()
    {
        var order = PendingOrder();
        var paymentId = Guid.NewGuid();

        order.ConfirmPayment(paymentId).Should().BeTrue();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.Payment.PaymentId.Should().Be(paymentId);
    }

    [Test]
    public void FailPayment_em_pedido_pendente_deve_marcar_falha_sem_PaymentId()
    {
        var order = PendingOrder();

        order.FailPayment().Should().BeTrue();

        order.Status.Should().Be(OrderStatus.Failed);
        order.Payment.PaymentId.Should().BeNull();
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
        order.Payment.PaymentId.Should().Be(paymentId);
    }

    [Test]
    public void Pedido_com_falha_nao_deve_mudar_com_novo_resultado()
    {
        var order = PendingOrder();
        order.FailPayment();

        order.ConfirmPayment(Guid.NewGuid()).Should().BeFalse();

        order.Status.Should().Be(OrderStatus.Failed);
        order.Payment.PaymentId.Should().BeNull();
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
        confirmed.Payment.EmailSentAt.Should().NotBeNull();
    }

    [Test]
    public void Segunda_marcacao_deve_manter_a_data_do_primeiro_envio()
    {
        var order = PendingOrder();
        order.ConfirmPayment(Guid.NewGuid());
        order.MarkPaymentEmailSent();
        var first = order.Payment.EmailSentAt;

        order.MarkPaymentEmailSent();

        order.Payment.EmailSentAt.Should().Be(first);
    }

    private static Order ConfirmedOrder()
    {
        var order = PendingOrder();
        order.ConfirmPayment(Guid.NewGuid());
        return order;
    }

    [Test]
    public void Falha_temporaria_do_email_conta_a_tentativa_e_agenda_a_proxima()
    {
        var order = ConfirmedOrder();
        var now = new DateTime(2026, 10, 16, 12, 0, 0);

        order.RegisterPaymentEmailRetry(now, TimeSpan.FromMinutes(2), "451 tente mais tarde");

        order.Payment.EmailAttempts.Should().Be(1);
        order.Payment.NextEmailAttemptAt.Should().Be(now.AddMinutes(2));
        order.Payment.EmailFailureReason.Should().Be("451 tente mais tarde");
        order.IsAwaitingPaymentEmailAt(now.AddMinutes(1)).Should().BeFalse();
        order.IsAwaitingPaymentEmailAt(now.AddMinutes(2)).Should().BeTrue();
    }

    [Test]
    public void Falha_definitiva_do_email_tira_o_pedido_da_espera()
    {
        var order = ConfirmedOrder();
        var now = DateTime.Now;

        order.MarkPaymentEmailFailed(now, "550 destinatário inexistente");

        order.Payment.EmailFailedAt.Should().Be(now);
        order.Payment.NextEmailAttemptAt.Should().BeNull();
        order.Payment.EmailFailureReason.Should().Be("550 destinatário inexistente");
        order.IsAwaitingPaymentEmailAt(now.AddDays(1)).Should().BeFalse();
    }

    [Test]
    public void Pedido_sem_tentativa_agendada_aguarda_o_email()
    {
        ConfirmedOrder().IsAwaitingPaymentEmail.Should().BeTrue();
    }
}
