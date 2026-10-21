namespace Orders.Tests.CommandHandlers;

[TestFixture]
public class OrderPaymentHandlersTests
{
    private Mock<IOrderRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IOrderPaymentNotifier> _notifier = null!;
    private OrderPaymentHandlers _handlers = null!;
    private Order _order = null!;

    [SetUp]
    public void SetUp()
    {
        _order = new Order(Guid.NewGuid());
        _unitOfWork = new Mock<IUnitOfWork>();
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);
        _repository.Setup(r => r.GetByNumberAsync(_order.OrderNumber, It.IsAny<CancellationToken>())).ReturnsAsync(_order);
        _notifier = new Mock<IOrderPaymentNotifier>();
        _handlers = new OrderPaymentHandlers(_repository.Object, _notifier.Object, NullLogger<OrderPaymentHandlers>.Instance);
    }

    [Test]
    public async Task Aprovado_deve_confirmar_salvar_e_notificar()
    {
        var paymentId = Guid.NewGuid();

        await _handlers.Handle(new ConfirmOrderPaymentCommand(_order.OrderNumber, paymentId), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Confirmed);
        _order.Payment.PaymentId.Should().Be(paymentId);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, OrderStatus.Confirmed, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Resultado_leva_o_numero_do_pedido_ao_span_corrente()
    {
        using var span = new Activity("payment-approved process").Start();

        await _handlers.Handle(new ConfirmOrderPaymentCommand(_order.OrderNumber, Guid.NewGuid()), CancellationToken.None);

        span.GetTagItem(OrderTelemetry.OrderNumberTag).Should().Be(_order.OrderNumber);
    }

    [Test]
    public async Task Recusado_deve_marcar_falha_salvar_e_notificar()
    {
        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "recusado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Failed);
        _order.Payment.PaymentId.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, OrderStatus.Failed, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedido_que_nao_esta_pendente_nao_deve_mudar_nem_notificar()
    {
        _order.ConfirmPayment(Guid.NewGuid(), 1);

        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "recusado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Confirmed);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_inexistente_deve_ser_ignorado_sem_notificar()
    {
        await _handlers.Handle(new ConfirmOrderPaymentCommand("X999", Guid.NewGuid()), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Nao_processado_deve_marcar_salvar_e_notificar()
    {
        await _handlers.Handle(new MarkOrderPaymentNotProcessedCommand(_order.OrderNumber), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.NotProcessed);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, OrderStatus.NotProcessed, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Recusa_depois_de_nao_processado_nao_deve_mudar_o_pedido()
    {
        _order.MarkPaymentNotProcessed(1);

        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "Pedido anulado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.NotProcessed);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Consulta_de_status_do_dono_devolve_o_status()
        => (await _handlers.Handle(new GetOrderStatusQuery(_order.OrderNumber, _order.CustomerId), CancellationToken.None))
            .Should().Be(new OrderPaymentState(OrderStatus.Pending.Name, 1));

    [Test]
    public async Task Consulta_de_status_de_outro_cliente_devolve_nulo()
        => (await _handlers.Handle(new GetOrderStatusQuery(_order.OrderNumber, Guid.NewGuid()), CancellationToken.None))
            .Should().BeNull();

    [Test]
    public async Task Consulta_de_status_de_numero_inexistente_devolve_nulo()
        => (await _handlers.Handle(new GetOrderStatusQuery("X999", _order.CustomerId), CancellationToken.None))
            .Should().BeNull();

    [Test]
    public async Task Resultado_atrasado_da_tentativa_anterior_nao_deve_mudar_nem_notificar()
    {
        _order.FailPayment(1);
        _order.RetryPayment(DateTime.Now);

        await _handlers.Handle(new ConfirmOrderPaymentCommand(_order.OrderNumber, Guid.NewGuid(), 1), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Pending);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Resultado_da_tentativa_atual_deve_mudar_e_notificar_com_a_tentativa()
    {
        _order.FailPayment(1);
        _order.RetryPayment(DateTime.Now);

        await _handlers.Handle(new ConfirmOrderPaymentCommand(_order.OrderNumber, Guid.NewGuid(), 2), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Confirmed);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, OrderStatus.Confirmed, 2, It.IsAny<CancellationToken>()), Times.Once);
    }
}
