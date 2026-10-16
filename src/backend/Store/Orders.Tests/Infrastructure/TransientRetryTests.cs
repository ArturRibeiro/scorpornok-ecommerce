using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Infrastructure.Consumers;
using Orders.Infrastructure.Messaging;
using Shared.Code;

namespace Orders.Tests.Infrastructure;

[TestFixture]
public class TransientRetryTests
{
    // Intervalos curtos: a política é a mesma, só não espera minutos no teste.
    private static readonly TimeSpan[] Short = Enumerable.Repeat(TimeSpan.FromMilliseconds(10), 7).ToArray();

    private static async Task<(ServiceProvider Provider, ITestHarness Harness)> StartAsync(IMemoryBus bus)
    {
        var provider = new ServiceCollection()
            .AddScoped(_ => bus)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<PaymentApprovedConsumer>();
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
    }

    [Test]
    public async Task Falha_passageira_ao_aplicar_o_resultado_nao_deve_ir_para_a_fila_de_erro()
    {
        var bus = new Mock<IMemoryBus>();
        bus.SetupSequence(b => b.SendAsync(It.IsAny<ConfirmOrderPaymentCommand>()))
            .ThrowsAsync(new InvalidOperationException("banco fora do ar"))
            .ThrowsAsync(new InvalidOperationException("banco fora do ar"))
            .Returns(Task.CompletedTask);
        var (provider, harness) = await StartAsync(bus.Object);
        await using var _ = provider;

        await harness.Bus.Publish(new PaymentApproved("A123", Guid.NewGuid()));

        (await harness.GetConsumerHarness<PaymentApprovedConsumer>().Consumed.Any<PaymentApproved>()).Should().BeTrue();
        await harness.InactivityTask;
        bus.Verify(b => b.SendAsync(It.IsAny<ConfirmOrderPaymentCommand>()), Times.Exactly(3));
        (await harness.Published.Any<Fault<PaymentApproved>>()).Should().BeFalse();
    }

    [Test]
    public async Task Resultado_invalido_deve_ir_para_a_fila_de_erro_sem_novas_tentativas()
    {
        var bus = new Mock<IMemoryBus>();
        bus.Setup(b => b.SendAsync(It.IsAny<ConfirmOrderPaymentCommand>()))
            .ThrowsAsync(new ArgumentException("dados inválidos"));
        var (provider, harness) = await StartAsync(bus.Object);
        await using var _ = provider;

        await harness.Bus.Publish(new PaymentApproved("A123", Guid.NewGuid()));

        (await harness.Published.Any<Fault<PaymentApproved>>()).Should().BeTrue();
        bus.Verify(b => b.SendAsync(It.IsAny<ConfirmOrderPaymentCommand>()), Times.Once);
    }
}
