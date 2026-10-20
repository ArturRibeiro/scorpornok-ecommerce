namespace Orders.Tests.Infrastructure;

[TestFixture]
public class ErrorQueuesHealthCheckTests
{
    // Responde a cada fila com a função recebida e guarda as URLs chamadas.
    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<string> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!.AbsolutePath);
            return Task.FromResult(respond(request));
        }
    }

    private static ErrorQueuesHealthCheck Check(FakeHandler handler, string? baseAddress = "http://rabbitmq:15672/")
        => new(new HttpClient(handler) { BaseAddress = baseAddress is null ? null : new Uri(baseAddress) },
            new ErrorQueues(["payment-approved_error", "payment-rejected_error"]));

    private static HttpResponseMessage Queue(int messages)
        => new(HttpStatusCode.OK) { Content = new StringContent($$"""{"name":"q","messages":{{messages}}}""", Encoding.UTF8, "application/json") };

    private static Task<HealthCheckResult> Run(ErrorQueuesHealthCheck check)
        => check.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

    [Test]
    public async Task Filas_vazias_ou_inexistentes_devem_ser_Healthy()
    {
        var handler = new FakeHandler(r => r.RequestUri!.AbsolutePath.Contains("payment-approved")
            ? Queue(0)
            : new HttpResponseMessage(HttpStatusCode.NotFound));

        (await Run(Check(handler))).Status.Should().Be(HealthStatus.Healthy);
        handler.Requests.Should().Equal("/api/queues/%2F/payment-approved_error", "/api/queues/%2F/payment-rejected_error");
    }

    [Test]
    public async Task Fila_com_mensagens_deve_ser_Unhealthy_citando_a_fila_e_a_quantidade()
    {
        var handler = new FakeHandler(r => r.RequestUri!.AbsolutePath.Contains("payment-rejected") ? Queue(3) : Queue(0));

        var result = await Run(Check(handler));

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Contain("payment-rejected_error=3");
        result.Data["payment-rejected_error"].Should().Be(3);
    }

    [Test]
    public async Task API_de_gerenciamento_fora_deve_ser_Degraded()
    {
        var handler = new FakeHandler(_ => throw new HttpRequestException("connection refused"));

        (await Run(Check(handler))).Status.Should().Be(HealthStatus.Degraded);
    }

    [Test]
    public async Task Sem_ManagementUrl_deve_ser_Degraded()
    {
        var handler = new FakeHandler(_ => Queue(0));

        (await Run(Check(handler, baseAddress: null))).Status.Should().Be(HealthStatus.Degraded);
        handler.Requests.Should().BeEmpty();
    }
}
