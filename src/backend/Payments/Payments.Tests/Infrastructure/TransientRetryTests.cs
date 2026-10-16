using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Payments.CommandHandlers.Payments;
using Payments.Infrastructure.Consumers;
using Payments.Infrastructure.Messaging;
using Shared.Code;
using Shared.Code.IntegrationEvents;

namespace Payments.Tests.Infrastructure;

[TestFixture]
public class TransientRetryTests
{
    // Intervalos curtos: a política é a mesma, só não espera minutos no teste.
    private static readonly TimeSpan[] Short = Enumerable.Repeat(TimeSpan.FromMilliseconds(10), 7).ToArray();

    private static PaymentRequested Request()
        => new(7, "A123", Guid.NewGuid(), 25m, 1, "tok_teste");

    private static async Task<(ServiceProvider Provider, ITestHarness Harness)> StartAsync(IMemoryBus bus)
    {
        var provider = new ServiceCollection()
            .AddScoped(_ => bus)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PaymentRequestedConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseTransientRetry(Short);
                    cfg.ConfigureEndpoints(context);
                });
            })
            .BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        return (provider, harness);
    }

    [Test]
    public void Intervalos_devem_somar_entre_4_e_6_minutos_e_ficar_abaixo_do_consumer_timeout()
    {
        var total = TransientRetry.Intervals.Aggregate(TimeSpan.Zero, (sum, interval) => sum + interval);

        total.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMinutes(4)).And.BeLessThanOrEqualTo(TimeSpan.FromMinutes(6));
        total.Should().BeLessThan(TimeSpan.FromMinutes(30));
        TransientRetry.Intervals.Should().BeInAscendingOrder();
    }

    [Test]
    public async Task Falha_passageira_deve_ser_processada_sem_ir_para_a_fila_de_erro()
    {
        var bus = new Mock<IMemoryBus>();
        bus.SetupSequence(b => b.SendAsync(It.IsAny<ProcessPaymentCommand>()))
            .ThrowsAsync(new InvalidOperationException("banco fora do ar"))
            .ThrowsAsync(new InvalidOperationException("banco fora do ar"))
            .Returns(Task.CompletedTask);
        var (provider, harness) = await StartAsync(bus.Object);
        await using var _ = provider;

        await harness.Bus.Publish(Request());

        (await harness.GetConsumerHarness<PaymentRequestedConsumer>().Consumed.Any<PaymentRequested>()).Should().BeTrue();
        await harness.InactivityTask;
        bus.Verify(b => b.SendAsync(It.IsAny<ProcessPaymentCommand>()), Times.Exactly(3));
        (await harness.Published.Any<Fault<PaymentRequested>>()).Should().BeFalse();
    }

    [Test]
    public async Task Mensagem_invalida_deve_ir_para_a_fila_de_erro_sem_novas_tentativas()
    {
        var bus = new Mock<IMemoryBus>();
        bus.Setup(b => b.SendAsync(It.IsAny<ProcessPaymentCommand>()))
            .ThrowsAsync(new ArgumentException("dados inválidos"));
        var (provider, harness) = await StartAsync(bus.Object);
        await using var _ = provider;

        await harness.Bus.Publish(Request());

        (await harness.Published.Any<Fault<PaymentRequested>>()).Should().BeTrue();
        bus.Verify(b => b.SendAsync(It.IsAny<ProcessPaymentCommand>()), Times.Once);
    }

    [Test]
    public async Task Falha_que_nao_passa_deve_ir_para_a_fila_de_erro_depois_de_todas_as_tentativas()
    {
        var bus = new Mock<IMemoryBus>();
        bus.Setup(b => b.SendAsync(It.IsAny<ProcessPaymentCommand>()))
            .ThrowsAsync(new InvalidOperationException("banco fora do ar"));
        var (provider, harness) = await StartAsync(bus.Object);
        await using var _ = provider;

        await harness.Bus.Publish(Request());

        (await harness.Published.Any<Fault<PaymentRequested>>()).Should().BeTrue();
        bus.Verify(b => b.SendAsync(It.IsAny<ProcessPaymentCommand>()), Times.Exactly(1 + Short.Length));
    }
}
