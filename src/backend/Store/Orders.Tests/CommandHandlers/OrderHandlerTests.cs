using Orders.CommandHandlers.Orders.Imp.CommandHandlers;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;
using Shared.Code.Models;

namespace Orders.Tests.CommandHandlers;

[TestFixture]
public class OrderHandlerTests
{
    private Mock<IOrderRepository> _repository = null!;
    private OrderHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(unitOfWork.Object);
        _repository.Setup(r => r.Save(It.IsAny<Order>())).Returns((Order order) => order);
        _handler = new OrderHandler(_repository.Object);
    }

    private static OrderItemMessageResponse Item(int productId, decimal unitPrice, int units)
        => new()
        {
            ProductId = productId,
            ProductName = $"Produto {productId}",
            PictureUrl = "http://img",
            UnitPrice = unitPrice,
            Units = units
        };

    private static CreateCommand Command(Guid userId, string city = "São Paulo", params OrderItemMessageResponse[] items)
        => new(
            userId,
            new OrderAddressMessageResponse
            {
                Street = "Rua A, 10", City = city, State = "SP", Country = "Brasil", ZipCode = "01000-000"
            },
            items,
            new CreditCardPaymentCommand(Guid.Empty, "Fulano", "4111111111111111", "12", "2030", "123", 25m));

    [Test]
    public async Task Pedido_valido_deve_ser_salvo_para_o_cliente_informado()
    {
        var userId = Guid.NewGuid();
        Order? saved = null;
        _repository.Setup(r => r.Save(It.IsAny<Order>())).Callback((Order o) => saved = o).Returns((Order o) => o);

        var result = await _handler.Handle(
            Command(userId, items: [Item(42, 10m, 2), Item(7, 5m, 1)]), CancellationToken.None);

        result.Success.Should().BeTrue();
        result.OrderNumber.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(OrderStatus.Pending.Name);
        result.Total.Should().Be(25m);
        result.Errors.Should().BeEmpty();
        saved.Should().NotBeNull();
        saved!.CustomerId.Should().Be(userId);
        saved.Items.Select(i => i.ProductId).Should().BeEquivalentTo([42, 7]);
    }

    [Test]
    public async Task Pedido_invalido_nao_deve_ser_salvo()
    {
        var result = await _handler.Handle(Command(Guid.NewGuid(), city: "", items: []), CancellationToken.None);

        result.Success.Should().BeFalse();
        result.OrderNumber.Should().BeNull();
        result.Errors.Should().Contain("Order has no items.");
        result.Errors.Should().Contain(error => error.Contains("City"));
        _repository.Verify(r => r.Save(It.IsAny<Order>()), Times.Never);
    }

    [Test]
    public async Task Pedido_sem_endereco_e_sem_itens_deve_devolver_erros_sem_excecao()
    {
        var command = new CreateCommand(Guid.NewGuid(), null!, null!,
            new CreditCardPaymentCommand(Guid.Empty, "Fulano", "4111111111111111", "12", "2030", "123", 0m));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        _repository.Verify(r => r.Save(It.IsAny<Order>()), Times.Never);
    }
}
