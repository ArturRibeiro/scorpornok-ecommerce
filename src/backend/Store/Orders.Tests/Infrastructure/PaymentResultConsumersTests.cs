using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Infrastructure.Consumers;
using Shared.Code;

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
}
