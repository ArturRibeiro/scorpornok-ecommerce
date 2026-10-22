namespace Customers.Tests.Infrastructure;

[TestFixture]
public class OrderPlacedConsumerTests
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
                x.AddConsumer<OrderPlacedConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseTransientRetry([TimeSpan.FromMilliseconds(10)]);
                    cfg.ConfigureEndpoints(context);
                });
            })
            .BuildServiceProvider(true);
        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    [TearDown]
    public async Task TearDown() => await _provider.DisposeAsync();

    private static OrderPlaced Message() => new("A123", Guid.NewGuid(), "cliente@exemplo.com", "Maria Souza",
        "21999998888", "52998224725",
        new ShippingAddress("Rua das Flores", "100", "Apto 12", "Centro", "Rio de Janeiro", "RJ", "20040020"),
        new DateTime(2026, 10, 18));

    [Test]
    public async Task OrderPlaced_deve_enviar_RecordPurchaseCommand_com_o_endereco_da_entrega()
    {
        var message = Message();

        await _harness.Bus.Publish(message);

        (await _harness.GetConsumerHarness<OrderPlacedConsumer>().Consumed.Any<OrderPlaced>()).Should().BeTrue();
        _bus.Verify(b => b.SendAsync(It.Is<RecordPurchaseCommand>(c =>
            c.OrderNumber == "A123" && c.CustomerId == message.CustomerId && c.Cpf == "52998224725"
            && c.ShippingAddress.City == "Rio de Janeiro" && c.ShippingAddress.Complement == "Apto 12")), Times.Once);
    }

    [Test]
    public async Task Falha_passageira_e_repetida()
    {
        _bus.SetupSequence(b => b.SendAsync(It.IsAny<RecordPurchaseCommand>()))
            .ThrowsAsync(new InvalidOperationException("banco fora"))
            .Returns(Task.CompletedTask);

        await _harness.Bus.Publish(Message());

        (await _harness.GetConsumerHarness<OrderPlacedConsumer>().Consumed.Any<OrderPlaced>()).Should().BeTrue();
        await Task.Delay(200);
        _bus.Verify(b => b.SendAsync(It.IsAny<RecordPurchaseCommand>()), Times.Exactly(2));
    }
}
