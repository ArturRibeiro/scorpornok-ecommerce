using Gateway.Payment.CommandHandlers.Payments;
using Gateway.Payment.Data.Consumers;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Code;
using Shared.Code.IntegrationEvents;

namespace Gateway.Payment.Tests.Data;

[TestFixture]
public class PaymentRequestedConsumerTests
{
    [Test]
    public async Task Deve_enviar_ProcessPaymentCommand_ao_receber_PaymentRequested()
    {
        var bus = new Mock<IMemoryBus>();
        await using var provider = new ServiceCollection()
            .AddScoped(_ => bus.Object)
            .AddMassTransitTestHarness(x => x.AddConsumer<PaymentRequestedConsumer>())
            .BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var card = new CardData("Fulano de Tal", "4111111111111111", "12", "2030", "123");
        await harness.Bus.Publish(new PaymentRequested(7, "A123", Guid.NewGuid(), 25m, 2, card));

        (await harness.GetConsumerHarness<PaymentRequestedConsumer>().Consumed.Any<PaymentRequested>()).Should().BeTrue();
        // AggregateId é gerado por mensagem, então a comparação é campo a campo.
        bus.Verify(b => b.SendAsync(It.Is<ProcessPaymentCommand>(c =>
            c.OrderId == 7 && c.OrderNumber == "A123" && c.Amount == 25m && c.Installments == 2 && c.Card == card)), Times.Once);
    }
}
