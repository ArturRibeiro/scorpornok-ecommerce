using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orders.CommandHandlers.Orders;
using Orders.Infrastructure.Email;

namespace Orders.Tests.Infrastructure;

[TestFixture]
public class OrderEmailDispatcherTests
{
    private Mock<IOrderRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IOrderEmailSender> _sender = null!;
    private OrderEmailDispatcher _dispatcher = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);
        _sender = new Mock<IOrderEmailSender>();
        var provider = new ServiceCollection()
            .AddScoped(_ => _repository.Object)
            .AddScoped(_ => _sender.Object)
            .BuildServiceProvider();
        _dispatcher = new OrderEmailDispatcher(provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new SmtpOptions()), NullLogger<OrderEmailDispatcher>.Instance);
    }

    private static Order ConfirmedOrder()
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@exemplo.com");
        order.ConfirmPayment(Guid.NewGuid());
        return order;
    }

    private void Returns(params Order[] orders)
        => _repository.Setup(r => r.GetAwaitingPaymentEmailAsync(OrderEmailDispatcher.BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

    [Test]
    public async Task Deve_enviar_e_marcar_cada_pedido()
    {
        var first = ConfirmedOrder();
        var second = ConfirmedOrder();
        Returns(first, second);

        var sent = await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        sent.Should().Be(2);
        _sender.Verify(s => s.SendPaymentResultAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        first.PaymentEmailSentAt.Should().NotBeNull();
        second.PaymentEmailSentAt.Should().NotBeNull();
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Falha_no_envio_nao_deve_marcar_e_deve_encerrar_a_rodada()
    {
        var first = ConfirmedOrder();
        var second = ConfirmedOrder();
        Returns(first, second);
        _sender.Setup(s => s.SendPaymentResultAsync(first, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP fora do ar"));

        var sent = await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        sent.Should().Be(0);
        first.PaymentEmailSentAt.Should().BeNull();
        _sender.Verify(s => s.SendPaymentResultAsync(second, It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Pedido_ja_marcado_nao_deve_receber_outro_email()
    {
        var order = ConfirmedOrder();
        order.MarkPaymentEmailSent();
        Returns(order);

        var sent = await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        sent.Should().Be(0);
        _sender.Verify(s => s.SendPaymentResultAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
