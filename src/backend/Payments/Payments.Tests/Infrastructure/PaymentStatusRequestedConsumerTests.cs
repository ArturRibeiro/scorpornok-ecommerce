using Payments.CommandHandlers.Payments;
using Payments.Infrastructure.Consumers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Code;
using Shared.Code.IntegrationEvents;

namespace Payments.Tests.Infrastructure;

[TestFixture]
public class PaymentStatusRequestedConsumerTests
{
    [Test]
    public async Task Deve_enviar_CheckPaymentStatusCommand_ao_receber_PaymentStatusRequested()
    {
        var bus = new Mock<IMemoryBus>();
        await using var provider = new ServiceCollection()
            .AddScoped(_ => bus.Object)
            .AddMassTransitTestHarness(x => x.AddConsumer<PaymentStatusRequestedConsumer>())
            .BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new PaymentStatusRequested("A123", 1));

        (await harness.GetConsumerHarness<PaymentStatusRequestedConsumer>().Consumed.Any<PaymentStatusRequested>()).Should().BeTrue();
        bus.Verify(b => b.SendAsync(It.Is<CheckPaymentStatusCommand>(c => c.OrderNumber == "A123" && c.Attempt == 1)), Times.Once);
    }
}
