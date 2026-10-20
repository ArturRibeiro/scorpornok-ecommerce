namespace Orders.Tests.Infrastructure;

[TestFixture]
public class PendingOrderWatcherTests
{
    private Mock<IOrderRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Mock<IIntegrationEventPublisher> _publisher = null!;
    private PendingOrderWatcher _watcher = null!;
    private readonly PaymentCheckOptions _options = new() { TimeoutMinutes = 15, IntervalSeconds = 60 };
    // Registra a ordem: publicações no outbox antes do SaveChanges que grava o StatusCheckedAt.
    private List<string> _calls = null!;

    [SetUp]
    public void SetUp()
    {
        _calls = [];
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => _calls.Add("save")).ReturnsAsync(true);
        _repository = new Mock<IOrderRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);
        _publisher = new Mock<IIntegrationEventPublisher>();
        _publisher.Setup(p => p.PublishAsync(It.IsAny<PaymentStatusRequested>(), It.IsAny<CancellationToken>()))
            .Callback((PaymentStatusRequested m, CancellationToken _) => _calls.Add("publish:" + m.OrderNumber))
            .Returns(Task.CompletedTask);
        var provider = new ServiceCollection()
            .AddScoped(_ => _repository.Object)
            .AddScoped(_ => _publisher.Object)
            .BuildServiceProvider();
        _watcher = new PendingOrderWatcher(provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(_options), NullLogger<PendingOrderWatcher>.Instance);
    }

    private void Returns(params Order[] orders)
        => _repository.Setup(r => r.GetPaymentCheckDueAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                PendingOrderWatcher.BatchSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

    [Test]
    public async Task Deve_buscar_com_o_prazo_e_o_intervalo_configurados()
    {
        Returns();
        var now = new DateTime(2026, 10, 16, 12, 0, 0);

        await _watcher.CheckDueOrdersAsync(now, CancellationToken.None);

        _repository.Verify(r => r.GetPaymentCheckDueAsync(now.AddMinutes(-15), now.AddSeconds(-60),
            PendingOrderWatcher.BatchSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Prazo_nao_atingido_nao_deve_consultar_nem_gravar()
    {
        // O repositório devolveu um pedido recente (ex.: relógios diferentes); a regra do domínio barra.
        Returns(new Order(Guid.NewGuid()));

        var checkedCount = await _watcher.CheckDueOrdersAsync(DateTime.Now.AddMinutes(5), CancellationToken.None);

        checkedCount.Should().Be(0);
        _calls.Should().BeEmpty();
    }

    [Test]
    public async Task Pedido_atrasado_deve_publicar_a_consulta_e_gravar_StatusCheckedAt_no_mesmo_save()
    {
        var order = new Order(Guid.NewGuid());
        Returns(order);
        var now = DateTime.Now.AddMinutes(16);

        var checkedCount = await _watcher.CheckDueOrdersAsync(now, CancellationToken.None);

        checkedCount.Should().Be(1);
        _calls.Should().Equal("publish:" + order.OrderNumber, "save");
        order.Payment.StatusCheckedAt.Should().Be(now);
    }

    [Test]
    public async Task Nova_consulta_so_depois_do_intervalo()
    {
        var order = new Order(Guid.NewGuid());
        Returns(order);
        var first = DateTime.Now.AddMinutes(16);
        await _watcher.CheckDueOrdersAsync(first, CancellationToken.None);
        _calls.Clear();

        (await _watcher.CheckDueOrdersAsync(first.AddSeconds(30), CancellationToken.None)).Should().Be(0);
        _calls.Should().BeEmpty();

        (await _watcher.CheckDueOrdersAsync(first.AddSeconds(60), CancellationToken.None)).Should().Be(1);
        _calls.Should().Equal("publish:" + order.OrderNumber, "save");
    }
}
