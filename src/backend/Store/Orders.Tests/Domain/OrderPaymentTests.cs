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
}
