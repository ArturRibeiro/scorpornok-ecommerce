namespace Orders.Tests.Infrastructure;

[TestFixture]
public class PaymentResultConsumersTests
{
    private Mock<IMemoryBus> _bus = null!;
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;

    [SetUp]
    public async Task SetUp()
    {
        _bus = new Mock<IMemoryBus>();
        _provider = new ServiceCollection()
            .AddScoped(_ => _bus.Object)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PaymentApprovedConsumer>();
                x.AddConsumer<PaymentRejectedConsumer>();
                x.AddConsumer<PaymentNotProcessedConsumer>();
                x.AddConsumer<PaymentStatusPendingConsumer>();
            })
            .BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    [TearDown]
    public async Task TearDown() => await _provider.DisposeAsync();

    [Test]
    public async Task PaymentApproved_deve_enviar_ConfirmOrderPaymentCommand()
    {
        var paymentId = Guid.NewGuid();

        await _harness.Bus.Publish(new PaymentApproved("A123", paymentId));

        (await _harness.GetConsumerHarness<PaymentApprovedConsumer>().Consumed.Any<PaymentApproved>()).Should().BeTrue();
        _bus.Verify(b => b.SendAsync(It.Is<ConfirmOrderPaymentCommand>(c =>
            c.OrderNumber == "A123" && c.PaymentId == paymentId)), Times.Once);
    }

    [Test]
    public async Task PaymentRejected_deve_enviar_FailOrderPaymentCommand()
    {
        await _harness.Bus.Publish(new PaymentRejected("A123", "Card declined"));

        (await _harness.GetConsumerHarness<PaymentRejectedConsumer>().Consumed.Any<PaymentRejected>()).Should().BeTrue();
        _bus.Verify(b => b.SendAsync(It.Is<FailOrderPaymentCommand>(c =>
            c.OrderNumber == "A123" && c.Reason == "Card declined")), Times.Once);
    }

    [Test]
    public async Task PaymentNotProcessed_deve_enviar_MarkOrderPaymentNotProcessedCommand()
    {
        await _harness.Bus.Publish(new PaymentNotProcessed("A123"));

        (await _harness.GetConsumerHarness<PaymentNotProcessedConsumer>().Consumed.Any<PaymentNotProcessed>()).Should().BeTrue();
        _bus.Verify(b => b.SendAsync(It.Is<MarkOrderPaymentNotProcessedCommand>(c => c.OrderNumber == "A123")), Times.Once);
    }

    [Test]
    public async Task PaymentStatusPending_so_deve_registrar_sem_mudar_o_pedido()
    {
        await _harness.Bus.Publish(new PaymentStatusPending("A123"));

        (await _harness.GetConsumerHarness<PaymentStatusPendingConsumer>().Consumed.Any<PaymentStatusPending>()).Should().BeTrue();
        _bus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Resultados_devem_repassar_a_tentativa()
    {
        var paymentId = Guid.NewGuid();

        await _harness.Bus.Publish(new PaymentApproved("A123", paymentId, 2));
        await _harness.Bus.Publish(new PaymentRejected("A456", "Card declined", 3));
        await _harness.Bus.Publish(new PaymentNotProcessed("A789", 2));

        (await _harness.GetConsumerHarness<PaymentApprovedConsumer>().Consumed.Any<PaymentApproved>()).Should().BeTrue();
        (await _harness.GetConsumerHarness<PaymentRejectedConsumer>().Consumed.Any<PaymentRejected>()).Should().BeTrue();
        (await _harness.GetConsumerHarness<PaymentNotProcessedConsumer>().Consumed.Any<PaymentNotProcessed>()).Should().BeTrue();
        _bus.Verify(b => b.SendAsync(It.Is<ConfirmOrderPaymentCommand>(c =>
            c.OrderNumber == "A123" && c.PaymentId == paymentId && c.Attempt == 2)), Times.Once);
        _bus.Verify(b => b.SendAsync(It.Is<FailOrderPaymentCommand>(c => c.OrderNumber == "A456" && c.Attempt == 3)), Times.Once);
        _bus.Verify(b => b.SendAsync(It.Is<MarkOrderPaymentNotProcessedCommand>(c =>
            c.OrderNumber == "A789" && c.Attempt == 2)), Times.Once);
    }

    [Test]
    public async Task Mensagem_antiga_sem_tentativa_vale_como_tentativa_1()
    {
        await _harness.Bus.Publish(new PaymentRejected("A123", "Card declined"));

        (await _harness.GetConsumerHarness<PaymentRejectedConsumer>().Consumed.Any<PaymentRejected>()).Should().BeTrue();
        _bus.Verify(b => b.SendAsync(It.Is<FailOrderPaymentCommand>(c => c.Attempt == 1)), Times.Once);
    }
}
