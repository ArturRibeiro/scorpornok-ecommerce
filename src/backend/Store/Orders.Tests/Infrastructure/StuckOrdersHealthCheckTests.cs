namespace Orders.Tests.Infrastructure;

[TestFixture]
public class StuckOrdersHealthCheckTests
{
    private readonly PaymentCheckOptions _options = new() { TimeoutMinutes = 15, AlertAfterMinutes = 10 };

    private (StuckOrdersHealthCheck Check, Mock<IOrderRepository> Repository) Create(int stuck)
    {
        var repository = new Mock<IOrderRepository>();
        repository.Setup(r => r.CountPendingCreatedBeforeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stuck);
        return (new StuckOrdersHealthCheck(repository.Object, Options.Create(_options)), repository);
    }

    [Test]
    public async Task Sem_pedidos_presos_deve_ser_Healthy_e_usar_prazo_mais_tolerancia()
    {
        var (check, repository) = Create(0);

        (await check.CheckHealthAsync(new HealthCheckContext())).Status.Should().Be(HealthStatus.Healthy);

        repository.Verify(r => r.CountPendingCreatedBeforeAsync(
            It.Is<DateTime>(d => (DateTime.Now - d - TimeSpan.FromMinutes(25)).Duration() < TimeSpan.FromSeconds(5)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Pedidos_presos_devem_ser_Unhealthy_com_a_quantidade()
    {
        var (check, _) = Create(2);

        var result = await check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().StartWith("2 pedido(s)");
        result.Data["stuckOrders"].Should().Be(2);
    }
}
