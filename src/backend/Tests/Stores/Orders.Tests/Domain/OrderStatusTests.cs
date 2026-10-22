namespace Orders.Tests.Domain;

[TestFixture]
public class OrderStatusTests
{
    [Test]
    public void Status_iguais_devem_ter_o_mesmo_hash()
    {
        OrderStatus.Confirmed.GetHashCode().Should().Be(OrderStatus.Confirmed.GetHashCode());
    }

    [Test]
    public void Status_diferentes_nao_devem_ser_iguais()
    {
        OrderStatus.Confirmed.Should().NotBe(OrderStatus.Failed);
    }

    [Test]
    public void Equals_com_null_ou_outro_tipo_deve_ser_falso()
    {
        OrderStatus.Pending.Equals("Pending").Should().BeFalse();
        OrderStatus.Pending.Equals(null).Should().BeFalse();
    }

    [Test]
    public void Status_deve_funcionar_como_chave_de_HashSet()
    {
        var statuses = new HashSet<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Failed, OrderStatus.Confirmed };

        statuses.Should().HaveCount(2);
    }
}
