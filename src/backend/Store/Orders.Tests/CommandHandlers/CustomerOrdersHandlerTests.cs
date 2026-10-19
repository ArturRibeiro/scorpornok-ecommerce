using System.Text.Json;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.History;

namespace Orders.Tests.CommandHandlers;

[TestFixture]
public class CustomerOrdersHandlerTests
{
    private static readonly Guid Owner = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private Mock<IOrderRepository> _repository = null!;
    private CustomerOrdersHandler _handler = null!;
    // Último skip/take pedido ao repositório.
    private (int Skip, int Take) _requested;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IOrderRepository>();
        _handler = new CustomerOrdersHandler(_repository.Object);
    }

    private static Order NewOrder(Guid? customerId = null)
    {
        var order = OrderBuilder.Create(customerId ?? Owner)
            .AddAddress("Rua A", "10", null, "Centro", "São Paulo", "SP", "01001-000")
            .AddEmail("cliente@exemplo.com")
            .Build();
        order.AddProduct([
            OrderItem.Create(42, "Camiseta", 100m, 0m, "/catalog-imgs/42.jpg", 2),
            OrderItem.Create(7, "Boné", 30m, 0m, "/catalog-imgs/7.jpg", 1)
        ]);
        return order;
    }

    private void RepositoryReturns(int count)
        => _repository.Setup(r => r.GetCustomerOrdersAsync(Owner, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback((Guid _, int skip, int take, CancellationToken _) => _requested = (skip, take))
            .ReturnsAsync((Guid _, int _, int take, CancellationToken _)
                => Enumerable.Range(0, Math.Min(count, take)).Select(_ => NewOrder()).ToList());

    private void DetailReturns(Order? order)
        => _repository.Setup(r => r.GetCustomerOrderAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order!);

    [Test]
    public async Task Pagina_cheia_com_mais_pedidos_tem_HasMore()
    {
        RepositoryReturns(25);

        var page = await _handler.Handle(new GetCustomerOrdersQuery(Owner, 2, 10), CancellationToken.None);

        _requested.Should().Be((10, 11));
        page.Items.Should().HaveCount(10);
        page.HasMore.Should().BeTrue();
        page.Page.Should().Be(2);
    }

    [Test]
    public async Task Ultima_pagina_nao_tem_HasMore()
    {
        RepositoryReturns(5);

        var page = await _handler.Handle(new GetCustomerOrdersQuery(Owner, 1, 10), CancellationToken.None);

        page.Items.Should().HaveCount(5);
        page.HasMore.Should().BeFalse();
    }

    [TestCase(0, 1)]
    [TestCase(500, CustomerOrdersHandler.MaxPageSize)]
    public async Task PageSize_fora_do_limite_e_ajustado(int pageSize, int expected)
    {
        RepositoryReturns(0);

        var page = await _handler.Handle(new GetCustomerOrdersQuery(Owner, 0, pageSize), CancellationToken.None);

        page.PageSize.Should().Be(expected);
        page.Page.Should().Be(1);
        _requested.Should().Be((0, expected + 1));
    }

    [Test]
    public async Task Linha_da_lista_soma_as_quantidades_e_usa_a_foto_do_primeiro_item()
    {
        RepositoryReturns(1);

        var item = (await _handler.Handle(new GetCustomerOrdersQuery(Owner, 1, 10), CancellationToken.None)).Items.Single();

        item.ItemCount.Should().Be(3);
        item.PictureUrl.Should().Be("/catalog-imgs/42.jpg");
        item.Total.Should().Be(230m);
        item.Status.Should().Be(OrderStatus.Pending.Name);
    }

    [Test]
    public async Task Detalhe_traz_itens_com_subtotal_e_endereco_da_compra()
    {
        var order = NewOrder();
        DetailReturns(order);

        var detail = await _handler.Handle(new GetCustomerOrderQuery(Owner, order.OrderNumber), CancellationToken.None);

        detail!.OrderNumber.Should().Be(order.OrderNumber);
        detail.Total.Should().Be(230m);
        detail.Items.Should().HaveCount(2);
        detail.Items[0].Should().BeEquivalentTo(new { ProductName = "Camiseta", UnitPrice = 100m, Quantity = 2, Subtotal = 200m });
        detail.Address.Should().BeEquivalentTo(new { Street = "Rua A", Number = "10", State = "SP", ZipCode = "01001000" });
        detail.Attempt.Should().Be(1);
        detail.MaxAttempts.Should().Be(Order.MaxPaymentAttempts);
        detail.CanRetryPayment.Should().BeFalse();
    }

    [Test]
    public async Task Recusado_com_tentativas_pode_pagar_de_novo()
    {
        var order = NewOrder();
        order.FailPayment(1);
        DetailReturns(order);

        var detail = await _handler.Handle(new GetCustomerOrderQuery(Owner, order.OrderNumber), CancellationToken.None);

        detail!.Status.Should().Be(OrderStatus.Failed.Name);
        detail.CanRetryPayment.Should().BeTrue();
    }

    [Test]
    public async Task Recusado_sem_tentativas_nao_pode_pagar_de_novo()
    {
        var order = NewOrder();
        for (var attempt = 1; attempt < Order.MaxPaymentAttempts; attempt++)
        {
            order.FailPayment(attempt);
            order.RetryPayment(DateTime.Now);
        }
        order.FailPayment(Order.MaxPaymentAttempts);
        DetailReturns(order);

        var detail = await _handler.Handle(new GetCustomerOrderQuery(Owner, order.OrderNumber), CancellationToken.None);

        detail!.Attempt.Should().Be(Order.MaxPaymentAttempts);
        detail.CanRetryPayment.Should().BeFalse();
    }

    [Test]
    public async Task Pedido_inexistente_ou_de_outro_cliente_e_nulo()
    {
        DetailReturns(null);
        (await _handler.Handle(new GetCustomerOrderQuery(Owner, "A000000000000"), CancellationToken.None))
            .Should().BeNull();

        // Mesmo que o repositório devolvesse o pedido de outro cliente, ele não sai.
        var other = NewOrder(Guid.NewGuid());
        DetailReturns(other);
        (await _handler.Handle(new GetCustomerOrderQuery(Owner, other.OrderNumber), CancellationToken.None))
            .Should().BeNull();
    }

    [Test]
    public async Task Respostas_nao_levam_pagamento_email_nem_cliente()
    {
        var order = NewOrder();
        order.ConfirmPayment(Guid.Parse("11111111-2222-3333-4444-555555555555"), 1);
        order.MarkPaymentEmailSent();
        DetailReturns(order);
        RepositoryReturns(1);

        var detail = await _handler.Handle(new GetCustomerOrderQuery(Owner, order.OrderNumber), CancellationToken.None);
        var page = await _handler.Handle(new GetCustomerOrdersQuery(Owner, 1, 10), CancellationToken.None);

        foreach (var json in new[] { JsonSerializer.Serialize(detail), JsonSerializer.Serialize(page) })
        {
            json.Should().NotContain("11111111-2222-3333-4444-555555555555")
                .And.NotContain("cliente@exemplo.com")
                .And.NotContain(Owner.ToString())
                .And.NotContainAny("PaymentId", "Email", "CustomerId", "Cpf", "Phone");
        }
    }
}
