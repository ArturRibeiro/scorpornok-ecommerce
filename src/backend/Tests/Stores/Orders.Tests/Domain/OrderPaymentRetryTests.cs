namespace Orders.Tests.Domain;

[TestFixture]
public class OrderPaymentRetryTests
{
    private static readonly DateTime Now = new(2026, 10, 16, 12, 0, 0);

    private static Order FailedOrder()
    {
        var order = new Order(Guid.NewGuid());
        order.FailPayment(1);
        return order;
    }

    [Test]
    public void Pedido_recusado_pode_tentar_de_novo_e_volta_a_pendente_na_tentativa_2()
    {
        var order = FailedOrder();

        order.CanRetryPayment.Should().BeTrue();
        order.RetryPayment(Now).Should().BeTrue();

        order.Status.Should().Be(OrderStatus.Pending);
        order.Payment.Attempt.Should().Be(2);
        order.Payment.AttemptRequestedAt.Should().Be(Now);
        order.AttemptStartedAt.Should().Be(Now);
    }

    [Test]
    public void Pedido_pendente_confirmado_ou_nao_processado_nao_pode_tentar_de_novo()
    {
        var pending = new Order(Guid.NewGuid());
        var confirmed = new Order(Guid.NewGuid());
        confirmed.ConfirmPayment(Guid.NewGuid(), 1);
        var notProcessed = new Order(Guid.NewGuid());
        notProcessed.MarkPaymentNotProcessed(1);

        foreach (var order in new[] { pending, confirmed, notProcessed })
        {
            order.CanRetryPayment.Should().BeFalse();
            order.RetryPayment(Now).Should().BeFalse();
            order.Payment.Attempt.Should().Be(1);
        }
        pending.Status.Should().Be(OrderStatus.Pending);
        confirmed.Status.Should().Be(OrderStatus.Confirmed);
        notProcessed.Status.Should().Be(OrderStatus.NotProcessed);
    }

    [Test]
    public void Depois_de_3_tentativas_recusadas_nao_ha_nova_tentativa()
    {
        var order = FailedOrder();
        order.RetryPayment(Now).Should().BeTrue();
        order.FailPayment(2).Should().BeTrue();
        order.RetryPayment(Now).Should().BeTrue();
        order.FailPayment(3).Should().BeTrue();

        order.Payment.Attempt.Should().Be(Order.MaxPaymentAttempts);
        order.CanRetryPayment.Should().BeFalse();
        order.RetryPayment(Now).Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Failed);
    }

    [Test]
    public void Resultado_de_tentativa_anterior_nao_deve_mudar_o_pedido()
    {
        var order = FailedOrder();
        order.RetryPayment(Now);

        // Resultados atrasados da tentativa 1 chegam depois da nova tentativa.
        order.ConfirmPayment(Guid.NewGuid(), 1).Should().BeFalse();
        order.FailPayment(1).Should().BeFalse();
        order.MarkPaymentNotProcessed(1).Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Pending);
        order.Payment.PaymentId.Should().BeNull();

        var paymentId = Guid.NewGuid();
        order.ConfirmPayment(paymentId, 2).Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.Payment.PaymentId.Should().Be(paymentId);
    }

    [Test]
    public void Nova_tentativa_libera_o_e_mail_do_novo_resultado()
    {
        var order = FailedOrder();
        order.RegisterPaymentEmailRetry(Now, TimeSpan.FromMinutes(1), "4xx");
        order.MarkPaymentEmailSent();
        order.IsAwaitingPaymentEmailAt(Now.AddHours(1)).Should().BeFalse();

        order.RetryPayment(Now);

        order.Payment.EmailSentAt.Should().BeNull();
        order.Payment.EmailAttempts.Should().Be(0);
        order.Payment.NextEmailAttemptAt.Should().BeNull();
        order.Payment.EmailFailedAt.Should().BeNull();
        order.IsAwaitingPaymentEmailAt(Now).Should().BeFalse(); // pendente: ainda sem resultado
        order.FailPayment(2);
        order.IsAwaitingPaymentEmailAt(Now).Should().BeTrue();
    }

    [Test]
    public void Prazo_da_consulta_conta_a_partir_da_nova_tentativa()
    {
        var order = FailedOrder();
        order.MarkPaymentStatusChecked(Now.AddMinutes(-30));
        var retryAt = Now.AddMinutes(-5);
        order.RetryPayment(retryAt);

        // O pedido é antigo, mas a tentativa 2 começou há 5 min: ainda dentro do prazo de 15 min.
        order.IsPaymentCheckDue(Now, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1)).Should().BeFalse();
        order.Payment.StatusCheckedAt.Should().BeNull();
        order.IsPaymentCheckDue(retryAt.AddMinutes(16), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(1))
            .Should().BeTrue();
    }
}
