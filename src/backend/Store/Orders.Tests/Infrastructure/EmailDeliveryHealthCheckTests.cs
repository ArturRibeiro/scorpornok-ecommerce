using Microsoft.Extensions.Diagnostics.HealthChecks;
using Orders.Infrastructure.Email;

namespace Orders.Tests.Infrastructure;

[TestFixture]
public class EmailDeliveryHealthCheckTests
{
    private Mock<IOrderRepository> _repository = null!;
    private EmailDeliveryState _state = null!;
    private EmailDeliveryHealthCheck _check = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IOrderRepository>();
        _state = new EmailDeliveryState();
        _check = new EmailDeliveryHealthCheck(_state, _repository.Object);
    }

    private void FailedEmails(int count)
        => _repository.Setup(r => r.CountEmailFailedSinceAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

    [Test]
    public async Task Servidor_fora_na_ultima_rodada_deve_ser_Unhealthy()
    {
        _state.MarkServerUnavailable("535 autenticação recusada");
        FailedEmails(0);

        var result = await _check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("535");
    }

    [Test]
    public async Task Falha_definitiva_nas_ultimas_24h_deve_ser_Degraded_com_a_quantidade()
    {
        FailedEmails(2);

        var result = await _check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Degraded);
        result.Data["failedEmails"].Should().Be(2);
        _repository.Verify(r => r.CountEmailFailedSinceAsync(
            It.Is<DateTime>(since => since > DateTime.Now.AddHours(-24).AddMinutes(-1) && since < DateTime.Now.AddHours(-23)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Sem_falhas_e_com_servidor_no_ar_deve_ser_Healthy()
    {
        FailedEmails(0);

        var result = await _check.CheckHealthAsync(new HealthCheckContext());

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Test]
    public async Task Servidor_de_volta_deve_sair_do_Unhealthy()
    {
        _state.MarkServerUnavailable("fora");
        _state.MarkServerAvailable();
        FailedEmails(0);

        (await _check.CheckHealthAsync(new HealthCheckContext())).Status.Should().Be(HealthStatus.Healthy);
    }
}
