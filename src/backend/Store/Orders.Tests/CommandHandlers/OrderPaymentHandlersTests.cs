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
        _order.PaymentId.Should().Be(paymentId);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Recusado_deve_marcar_falha_salvar_e_notificar()
    {
        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "recusado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Failed);
        _order.PaymentId.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(_order.OrderNumber, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedido_que_nao_esta_pendente_nao_deve_mudar_nem_notificar()
    {
        _order.ConfirmPayment(Guid.NewGuid());

        await _handlers.Handle(new FailOrderPaymentCommand(_order.OrderNumber, "recusado"), CancellationToken.None);

        _order.Status.Should().Be(OrderStatus.Confirmed);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_inexistente_deve_ser_ignorado_sem_notificar()
    {
        await _handlers.Handle(new ConfirmOrderPaymentCommand("X999", Guid.NewGuid()), CancellationToken.None);

        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.NotifyAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Consulta_de_status_deve_devolver_o_status_ou_nulo()
    {
        (await _handlers.Handle(new GetOrderStatusQuery(_order.OrderNumber), CancellationToken.None))
            .Should().Be(OrderStatus.Pending.Name);
        (await _handlers.Handle(new GetOrderStatusQuery("X999"), CancellationToken.None))
            .Should().BeNull();
    }
}
