using Orders.CommandHandlers.Orders;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;

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
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, OrderStatus.Confirmed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Recusado_deve_marcar_falha_salvar_e_notificar()
    {
        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "recusado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Failed);
        _order.Payment.PaymentId.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, OrderStatus.Failed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedido_que_nao_esta_pendente_nao_deve_mudar_nem_notificar()
    {
        _order.ConfirmPayment(Guid.NewGuid());

        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "recusado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Confirmed);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_inexistente_deve_ser_ignorado_sem_notificar()
    {
        await _handlers.Handle(new ConfirmOrderPaymentCommand("X999", Guid.NewGuid()), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Nao_processado_deve_marcar_salvar_e_notificar()
    {
        await _handlers.Handle(new MarkOrderPaymentNotProcessedCommand(_order.OrderNumber), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.NotProcessed);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, OrderStatus.NotProcessed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Recusa_depois_de_nao_processado_nao_deve_mudar_o_pedido()
    {
        _order.MarkPaymentNotProcessed();

        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "Pedido anulado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.NotProcessed);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Consulta_de_status_do_dono_devolve_o_status()
        => (await _handlers.Handle(new GetOrderStatusQuery(_order.OrderNumber, _order.CustomerId), CancellationToken.None))
            .Should().Be(OrderStatus.Pending.Name);

    [Test]
    public async Task Consulta_de_status_de_outro_cliente_devolve_nulo()
        => (await _handlers.Handle(new GetOrderStatusQuery(_order.OrderNumber, Guid.NewGuid()), CancellationToken.None))
            .Should().BeNull();

    [Test]
    public async Task Consulta_de_status_de_numero_inexistente_devolve_nulo()
        => (await _handlers.Handle(new GetOrderStatusQuery("X999", _order.CustomerId), CancellationToken.None))
            .Should().BeNull();
}
