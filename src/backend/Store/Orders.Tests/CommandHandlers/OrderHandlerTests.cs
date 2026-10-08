namespace Orders.Tests.CommandHandlers;

[TestFixture]
public class OrderHandlerTests
{
    private Mock<IOrderRepository> _repository = null!;
    private Mock<IIntegrationEventPublisher> _publisher = null!;
    private OrderHandler _handler = null!;
    // Registra a ordem das chamadas: begin/commit da transação, add (Save), save (SaveEntitiesAsync), publish.
    private List<string> _calls = null!;

    [SetUp]
    public void SetUp()
    {
        _calls = [];
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save")).ReturnsAsync(true);
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(unitOfWork.Object);
        _repository.Setup(r => r.Save(It.IsAny<Order>()))
            .Callback(() => _calls.Add("add")).Returns((Order order) => order);
        _repository.Setup(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task> work, CancellationToken _) =>
            {
                _calls.Add("begin");
                await work();
                _calls.Add("commit");
            });
        _publisher = new Mock<IIntegrationEventPublisher>();
        _publisher.Setup(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("publish")).Returns(Task.CompletedTask);
        _handler = new OrderHandler(_repository.Object, _publisher.Object, NullLogger<OrderHandler>.Instance);
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

    private static CreditCardPaymentCommand Card()
        => new(Guid.Empty, "Fulano", "4111111111111111", "12", "2030", "123", 1m, 3);

    private static CreateCommand Command(Guid userId, string city = "São Paulo", params OrderItemMessageResponse[] items)
        => new(
            userId,
            new OrderAddressMessageResponse
            {
                Street = "Rua A, 10", City = city, State = "SP", Country = "Brasil", ZipCode = "01000-000"
            },
            items,
            Card(),
            "cliente@exemplo.com");

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
        saved.Email.Should().Be("cliente@exemplo.com");
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

    [Test]
    public async Task Pedido_valido_deve_solicitar_o_pagamento_com_o_total_do_pedido()
    {
        var userId = Guid.NewGuid();

        var result = await _handler.Handle(
            Command(userId, items: [Item(42, 10m, 2), Item(7, 5m, 1)]), CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(
            It.Is<PaymentRequested>(m =>
                m.OrderNumber == result.OrderNumber && m.CustomerId == userId && m.Amount == 25m
                && m.Installments == 3 && m.Card.CardNumber == "4111111111111111" && m.Card.Cvv == "123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedido_invalido_nao_deve_solicitar_pagamento()
    {
        await _handler.Handle(Command(Guid.NewGuid(), items: []), CancellationToken.None);

        _publisher.Verify(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_valido_deve_publicar_antes_do_ultimo_save_dentro_da_transacao()
    {
        await _handler.Handle(Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]), CancellationToken.None);

        // O primeiro save gera o Id do pedido; a mensagem do outbox só é gravada no save seguinte.
        _calls.Should().Equal("begin", "add", "save", "publish", "save", "commit");
    }

    [Test]
    public async Task Falha_ao_publicar_deve_desfazer_o_registro_do_pedido()
    {
        _publisher.Setup(p => p.PublishAsync(It.IsAny<PaymentRequested>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("falha ao gravar no outbox"));

        var act = () => _handler.Handle(Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _calls.Should().NotContain("commit");
    }

    [Test]
    public async Task Pedido_sem_cartao_deve_ser_salvo_sem_solicitar_pagamento()
    {
        var command = Command(Guid.NewGuid(), items: [Item(42, 10m, 1)]) with { Card = null! };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        _calls.Should().Equal("begin", "add", "save", "commit");
    }

    [Test]
    public async Task Pedido_invalido_nao_deve_abrir_transacao()
    {
        await _handler.Handle(Command(Guid.NewGuid(), items: []), CancellationToken.None);

        _repository.Verify(r => r.ExecuteInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
