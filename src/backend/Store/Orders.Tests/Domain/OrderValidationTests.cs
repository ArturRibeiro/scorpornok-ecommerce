namespace Orders.Tests.Domain;

[TestFixture]
public class OrderValidationTests
{
    private static OrderAddress ValidAddress(string city = "São Paulo")
        => OrderAddress.Factory.Create("Rua A, 10", city, "SP", "Brasil", "01000-000");

    private static OrderItem Item(int quantity = 1, decimal unitPrice = 10m)
        => OrderItem.Create(42, "Produto", unitPrice, 0m, "http://img", quantity);

    private static Order CreateOrder(OrderAddress address, params OrderItem[] items)
        => CreateOrder(address, "cliente@exemplo.com", items);

    private static Order CreateOrder(OrderAddress address, string email, params OrderItem[] items)
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail(email);
        order.AddAddress(address);
        order.AddProduct(items);
        return order;
    }

    [Test]
    public void Pedido_valido_sem_PaymentId_deve_ser_valido()
    {
        var order = CreateOrder(ValidAddress(), Item());

        order.IsValid().Should().BeTrue();
        order.PaymentId.Should().BeNull();
        order.Errors.Should().BeEmpty();
    }

    [Test]
    public void Pedido_sem_itens_deve_ser_invalido()
    {
        var order = CreateOrder(ValidAddress());

        order.IsValid().Should().BeFalse();
        order.Errors.Should().Contain("Order has no items.");
    }

    [Test]
    public void Item_com_quantidade_zero_deve_invalidar_o_pedido()
    {
        var order = CreateOrder(ValidAddress(), Item(quantity: 0));

        order.IsValid().Should().BeFalse();
        order.Errors.Should().Contain(error => error.Contains("Units"));
    }

    [Test]
    public void Endereco_sem_cidade_deve_invalidar_o_pedido()
    {
        var order = CreateOrder(ValidAddress(city: ""), Item());

        order.IsValid().Should().BeFalse();
        order.Errors.Should().Contain(error => error.Contains("City"), string.Join(" | ", order.Errors));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("cliente@")]
    [TestCase("cliente")]
    public void Email_ausente_ou_invalido_deve_invalidar_o_pedido(string? email)
    {
        var order = CreateOrder(ValidAddress(), email!, Item());

        order.IsValid().Should().BeFalse();
        order.Errors.Should().Contain("Invalid email.");
    }

    [Test]
    public void Email_valido_deve_ser_guardado_sem_espacos()
    {
        var order = CreateOrder(ValidAddress(), " cliente@exemplo.com ", Item());

        order.IsValid().Should().BeTrue();
        order.Email.Should().Be("cliente@exemplo.com");
    }
}
