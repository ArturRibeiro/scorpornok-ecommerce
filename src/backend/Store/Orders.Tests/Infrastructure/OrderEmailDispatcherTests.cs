namespace Orders.Tests.Infrastructure;

[TestFixture]
public class OrderEmailDispatcherTests
{
    private Mock<IOrderRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IOrderEmailSender> _sender = null!;
    private EmailDeliveryState _state = null!;
    private SmtpOptions _options = null!;
    private OrderEmailDispatcher _dispatcher = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);
        _sender = new Mock<IOrderEmailSender>();
        _state = new EmailDeliveryState();
        _options = new SmtpOptions { PollSeconds = 5, MaxEmailAttempts = 3, RetryBaseSeconds = 60, RetryMaxSeconds = 600 };
        var provider = new ServiceCollection()
            .AddScoped(_ => _repository.Object)
            .AddScoped(_ => _sender.Object)
            .BuildServiceProvider();
        _dispatcher = new OrderEmailDispatcher(provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(_options), _state, NullLogger<OrderEmailDispatcher>.Instance);
    }

    private static Order ConfirmedOrder()
    {
        var order = new Order(Guid.NewGuid());
        order.AddEmail("cliente@exemplo.com");
        order.ConfirmPayment(Guid.NewGuid(), 1);
        return order;
    }

    private void Returns(params Order[] orders)
        => _repository.Setup(r => r.GetAwaitingPaymentEmailAsync(It.IsAny<DateTime>(), OrderEmailDispatcher.BatchSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

    private void Fails(Order order, EmailDeliveryFailure kind, string reason = "falha")
        => _sender.Setup(s => s.SendPaymentResultAsync(order, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EmailDeliveryException(kind, reason));

    [Test]
    public async Task Deve_enviar_e_marcar_cada_pedido()
    {
        var first = ConfirmedOrder();
        var second = ConfirmedOrder();
        Returns(first, second);

        var result = await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        result.Should().Be(new EmailDispatchResult(2, ServerUnavailable: false));
        first.Payment.EmailSentAt.Should().NotBeNull();
        second.Payment.EmailSentAt.Should().NotBeNull();
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Test]
    public async Task Destinatario_recusado_fica_em_falha_definitiva_e_nao_bloqueia_o_seguinte()
    {
        var refused = ConfirmedOrder();
        var next = ConfirmedOrder();
        Returns(refused, next);
        Fails(refused, EmailDeliveryFailure.Recipient, "550 No such user");

        var result = await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        result.Sent.Should().Be(1);
        refused.Payment.EmailFailedAt.Should().NotBeNull();
        refused.Payment.EmailFailureReason.Should().Be("550 No such user");
        refused.IsAwaitingPaymentEmail.Should().BeFalse();
        next.Payment.EmailSentAt.Should().NotBeNull();
    }

    [Test]
    public async Task Falha_temporaria_agenda_a_proxima_tentativa_com_intervalo_crescente()
    {
        var order = ConfirmedOrder();
        Returns(order);
        Fails(order, EmailDeliveryFailure.Message, "451 tente mais tarde");

        var before = DateTime.Now;
        await _dispatcher.DispatchPendingAsync(CancellationToken.None);
        var firstDelay = order.Payment.NextEmailAttemptAt!.Value - before;

        // Força a próxima rodada como se o intervalo já tivesse passado.
        typeof(OrderPayment).GetProperty(nameof(OrderPayment.NextEmailAttemptAt))!
            .SetValue(order.Payment, (DateTime?)DateTime.Now.AddSeconds(-1));
        var beforeSecond = DateTime.Now;
        await _dispatcher.DispatchPendingAsync(CancellationToken.None);
        var secondDelay = order.Payment.NextEmailAttemptAt!.Value - beforeSecond;

        order.Payment.EmailAttempts.Should().Be(2);
        order.Payment.EmailFailedAt.Should().BeNull();
        firstDelay.Should().BeCloseTo(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(2));
        secondDelay.Should().BeCloseTo(TimeSpan.FromSeconds(120), TimeSpan.FromSeconds(2));
    }

    [Test]
    public async Task Limite_de_tentativas_encerra_em_falha_definitiva()
    {
        var order = ConfirmedOrder();
        Returns(order);
        Fails(order, EmailDeliveryFailure.Message);

        for (var round = 0; round < _options.MaxEmailAttempts; round++)
        {
            typeof(OrderPayment).GetProperty(nameof(OrderPayment.NextEmailAttemptAt))!
                .SetValue(order.Payment, (DateTime?)null);
            await _dispatcher.DispatchPendingAsync(CancellationToken.None);
        }

        order.Payment.EmailAttempts.Should().Be(_options.MaxEmailAttempts);
        order.Payment.EmailFailedAt.Should().NotBeNull();
        order.IsAwaitingPaymentEmail.Should().BeFalse();
    }

    [Test]
    public async Task Servidor_fora_nao_altera_os_pedidos_e_aumenta_a_espera()
    {
        var first = ConfirmedOrder();
        var second = ConfirmedOrder();
        Returns(first, second);
        Fails(first, EmailDeliveryFailure.Server, "535 autenticação recusada");

        var result = await _dispatcher.DispatchPendingAsync(CancellationToken.None);
        var waitAfterFirst = _dispatcher.NextWait;
        await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        result.Should().Be(new EmailDispatchResult(0, ServerUnavailable: true));
        first.Payment.EmailAttempts.Should().Be(0);
        first.Payment.EmailFailedAt.Should().BeNull();
        _sender.Verify(s => s.SendPaymentResultAsync(second, It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _state.ServerUnavailable.Should().BeTrue();
        waitAfterFirst.Should().Be(TimeSpan.FromSeconds(10));
        _dispatcher.NextWait.Should().Be(TimeSpan.FromSeconds(20));
    }

    [Test]
    public async Task Espera_do_servidor_fora_para_no_teto()
    {
        var order = ConfirmedOrder();
        Returns(order);
        Fails(order, EmailDeliveryFailure.Server);

        for (var round = 0; round < 20; round++)
            await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        _dispatcher.NextWait.Should().Be(TimeSpan.FromSeconds(_options.RetryMaxSeconds));
    }

    [Test]
    public async Task Recuperacao_do_servidor_volta_a_espera_normal()
    {
        var order = ConfirmedOrder();
        Returns(order);
        Fails(order, EmailDeliveryFailure.Server);
        await _dispatcher.DispatchPendingAsync(CancellationToken.None);
        _sender.Reset();

        var result = await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        result.Sent.Should().Be(1);
        _state.ServerUnavailable.Should().BeFalse();
        _dispatcher.NextWait.Should().Be(TimeSpan.FromSeconds(_options.PollSeconds));
    }

    [Test]
    public async Task Pedido_ja_marcado_nao_deve_receber_outro_email()
    {
        var order = ConfirmedOrder();
        order.MarkPaymentEmailSent();
        Returns(order);

        var result = await _dispatcher.DispatchPendingAsync(CancellationToken.None);

        result.Sent.Should().Be(0);
        _sender.Verify(s => s.SendPaymentResultAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
