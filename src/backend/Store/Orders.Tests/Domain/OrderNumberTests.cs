namespace Orders.Tests.Domain;

[TestFixture]
public class OrderNumberTests
{
    [Test]
    public void Numero_deve_ser_A_seguido_de_12_digitos()
    {
        new Order(Guid.NewGuid()).OrderNumber.Should().MatchRegex(@"^A\d{12}$");
        OrderNumber.New().Should().MatchRegex(@"^A[1-9]\d{11}$");
    }

    [Test]
    public void Mil_pedidos_em_sequencia_devem_ter_numeros_distintos()
    {
        var numbers = Enumerable.Range(0, 1000).Select(_ => new Order(Guid.NewGuid()).OrderNumber).ToList();

        numbers.Should().OnlyHaveUniqueItems();
    }
}
