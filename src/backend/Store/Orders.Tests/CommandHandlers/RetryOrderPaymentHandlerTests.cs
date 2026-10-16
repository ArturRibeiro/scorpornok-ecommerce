using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;

namespace Orders.Tests.CommandHandlers;

[TestFixture]
public class RetryOrderPaymentHandlerTests
{
    private Mock<IOrderRepository> _repository = null!;
    private Mock<IIntegrationEventPublisher> _publisher = null!;
    private RetryOrderPaymentHandler _handler = null!;
    private Order _order = null!;
    // Registra a ordem das chamadas: begin/commit da transação, publish e save.
    private List<string> _calls = null!;

    [SetUp]
    public void SetUp()
    {
        _calls = [];
        _order = new Order(Guid.NewGuid());
        _order.AddProduct([OrderItem.Create(42, "Camiseta", 10m, 0m, "/catalog-imgs/42.jpg", 2)]);
        _order.FailPayment(1);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save")).ReturnsAsync(true);
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(unitOfWork.Object);
        _repository.Setup(r => r.GetByNumberAsync(_order.OrderNumber, It.IsAny<CancellationToken>())).ReturnsAsync(_order);
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
        _handler = new RetryOrderPaymentHandler(_repository.Object, _publisher.Object,
            NullLogger<RetryOrderPaymentHandler>.Instance);
    }

    private RetryOrderPaymentCommand Command(Guid? customerId = null, string? cardToken = "tok_novo")
        => new(_order.OrderNumber, customerId ?? _order.CustomerId, new PaymentMethod(cardToken, 2));

    [Test]
    public async Task Pedido_recusado_do_dono_volta_a_pendente_e_solicita_a_tentativa_2_no_outbox()
    {
        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.Outcome.Should().Be(RetryOrderPaymentOutcome.Accepted);
        result.Attempt.Should().Be(2);
        result.Status.Should().Be(OrderStatus.Pending.Name);
        _order.Status.Should().Be(OrderStatus.Pending);
        _publisher.Verify(p => p.PublishAsync(It.Is<PaymentRequested>(m =>
                m.OrderNumber == _order.OrderNumber && m.Attempt == 2 && m.Amount == 20m
                && m.CardToken == "tok_novo" && m.Installments == 2 && m.CustomerId == _order.CustomerId),
            It.IsAny<CancellationToken>()), Times.Once);
        // A mensagem é publicada (gravada no outbox) antes do SaveChanges, dentro da transação.
        _calls.Should().Equal("begin", "publish", "save", "commit");
    }

    [Test]
    public async Task Pedido_de_outro_cliente_ou_inexistente_nao_e_encontrado()
    {
        (await _handler.Handle(Command(customerId: Guid.NewGuid()), CancellationToken.None))
            .Outcome.Should().Be(RetryOrderPaymentOutcome.NotFound);
        (await _handler.Handle(new RetryOrderPaymentCommand("A000000000000", _order.CustomerId,
                new PaymentMethod("tok_novo")), CancellationToken.None))
            .Outcome.Should().Be(RetryOrderPaymentOutcome.NotFound);

        _order.Status.Should().Be(OrderStatus.Failed);
        _calls.Should().BeEmpty();
    }

    [Test]
    public async Task Sem_cartao_e_invalido_sem_mudar_o_pedido()
    {
        var result = await _handler.Handle(Command(cardToken: ""), CancellationToken.None);

        result.Outcome.Should().Be(RetryOrderPaymentOutcome.Invalid);
        result.Errors.Should().Contain("Informe o cartão.");
        _order.Status.Should().Be(OrderStatus.Failed);
        _calls.Should().BeEmpty();
    }

    [Test]
    public async Task Pedido_que_nao_esta_recusado_e_conflito()
    {
        _order.RetryPayment(DateTime.Now); // tentativa 2, pendente

        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.Outcome.Should().Be(RetryOrderPaymentOutcome.Conflict);
        result.Errors.Should().ContainSingle(e => e.Contains("recusado"));
        _calls.Should().BeEmpty();
    }

    [Test]
    public async Task Tentativas_esgotadas_e_conflito()
    {
        _order.RetryPayment(DateTime.Now);
        _order.FailPayment(2);
        _order.RetryPayment(DateTime.Now);
        _order.FailPayment(3);

        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.Outcome.Should().Be(RetryOrderPaymentOutcome.Conflict);
        result.Errors.Should().ContainSingle(e => e.Contains("acabaram"));
        _order.Payment.Attempt.Should().Be(3);
        _calls.Should().BeEmpty();
    }
}
