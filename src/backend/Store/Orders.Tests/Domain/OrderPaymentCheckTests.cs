namespace Orders.Tests.Domain;

[TestFixture]
public class OrderPaymentCheckTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    private static Order Pending()
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@exemplo.com");
        return order;
    }

    [Test]
    public void Pedido_pendente_antes_do_prazo_nao_deve_ser_consultado()
    {
        Pending().IsPaymentCheckDue(DateTime.Now.AddMinutes(14), Timeout, Interval).Should().BeFalse();
    }

    [Test]
    public void Pedido_pendente_alem_do_prazo_deve_ser_consultado()
    {
        Pending().IsPaymentCheckDue(DateTime.Now.AddMinutes(16), Timeout, Interval).Should().BeTrue();
    }

    [Test]
    public void Nova_consulta_so_depois_do_intervalo()
    {
        var order = Pending();
        var checkedAt = DateTime.Now.AddMinutes(16);
        order.MarkPaymentStatusChecked(checkedAt);

        order.IsPaymentCheckDue(checkedAt.AddSeconds(30), Timeout, Interval).Should().BeFalse();
        order.IsPaymentCheckDue(checkedAt.AddMinutes(1), Timeout, Interval).Should().BeTrue();
        order.Payment.StatusCheckedAt.Should().Be(checkedAt);
    }

    [Test]
    public void Pedido_com_desfecho_nao_deve_ser_consultado()
    {
        var order = Pending();
        order.ConfirmPayment(Guid.NewGuid());

        order.IsPaymentCheckDue(DateTime.Now.AddHours(1), Timeout, Interval).Should().BeFalse();
    }

    [Test]
    public void MarkPaymentNotProcessed_deve_agir_so_sobre_pedido_pendente()
    {
        var order = Pending();

        order.MarkPaymentNotProcessed().Should().BeTrue();
        order.Status.Should().Be(OrderStatus.NotProcessed);
        order.IsAwaitingPaymentEmail.Should().BeTrue();

        order.FailPayment().Should().BeFalse();
        order.ConfirmPayment(Guid.NewGuid()).Should().BeFalse();
        order.MarkPaymentNotProcessed().Should().BeFalse();
        order.Status.Should().Be(OrderStatus.NotProcessed);
    }

    [Test]
    public void Pedido_confirmado_nao_deve_virar_nao_processado()
    {
        var order = Pending();
        order.ConfirmPayment(Guid.NewGuid());

        order.MarkPaymentNotProcessed().Should().BeFalse();
        order.Status.Should().Be(OrderStatus.Confirmed);
    }
}
