namespace Frameworker.Observability.Tests;

[TestFixture]
public class ObservabilityTests
{
    [Test]
    public async Task Sem_endpoint_nenhum_exportador_e_registrado()
    {
        await using var host = await TelemetryHost.StartAsync();

        ObservabilityExtensions.IsExportEnabled(host.Services.GetRequiredService<IConfiguration>()).Should().BeFalse();
        RegisteredOtlpServices(host).Should().BeEmpty();
    }

    [Test]
    public async Task Com_endpoint_o_exportador_OTLP_e_registrado()
    {
        await using var host = await TelemetryHost.StartAsync(new()
        {
            [ObservabilityExtensions.EndpointVariable] = "http://localhost:4317"
        });

        RegisteredOtlpServices(host).Should().NotBeEmpty();
    }

    [Test]
    public async Task Requisicao_gera_span_com_o_nome_do_servico()
    {
        await using var host = await TelemetryHost.StartAsync();

        (await host.Client.GetStringAsync("/ping")).Should().Be("pong");
        await host.FlushAsync("GET /ping");

        host.Spans.Should().ContainSingle(span => span.DisplayName == "GET /ping");
        var resource = host.Services.GetRequiredService<TracerProvider>().GetResource();
        resource.Attributes.Should().Contain(new KeyValuePair<string, object>("service.name", "test-api"));
        resource.Attributes.Select(a => a.Key).Should().Contain(["service.version", "deployment.environment.name"]);
    }

    [Test]
    public async Task Log_durante_a_requisicao_leva_o_trace_dela()
    {
        await using var host = await TelemetryHost.StartAsync();

        await host.Client.GetStringAsync("/log");
        await host.FlushAsync("GET /log");

        var span = host.Spans.Should().ContainSingle(s => s.DisplayName == "GET /log").Subject;
        var log = host.Logs.Should().ContainSingle(l => l.FormattedMessage == "Pedido A123 registrado.").Subject;
        log.TraceId.Should().Be(span.TraceId);
        log.SpanId.Should().Be(span.SpanId);
    }

    [TestCase("mutation", "PlaceOrder", "mutation PlaceOrder")]
    [TestCase("query", "", "query")]
    public async Task Requisicao_GraphQL_leva_o_nome_da_operacao(string type, string name, string expected)
    {
        await using var host = await TelemetryHost.StartAsync();

        await host.Client.PostAsync($"/graphql?type={type}&name={name}", null);
        await host.FlushAsync(expected);

        var request = host.Spans.Should().ContainSingle(span => span.Kind == ActivityKind.Server).Subject;
        request.DisplayName.Should().Be(expected);
        request.GetTagItem("http.route").Should().Be("/graphql");
    }

    [Test]
    public async Task Health_check_nao_gera_span()
    {
        await using var host = await TelemetryHost.StartAsync();

        await host.Client.GetStringAsync("/health/live");
        await host.Client.GetStringAsync("/ping");
        await host.FlushAsync("GET /ping");

        host.Spans.Select(span => span.DisplayName).Should().Equal("GET /ping");
    }

    [Test]
    public async Task Token_na_query_nao_aparece_no_span()
    {
        const string token = "eyJhbGciOiJSUzI1NiJ9.segredo";
        await using var host = await TelemetryHost.StartAsync();

        await host.Client.GetStringAsync($"/hubs/orders/negotiate?access_token={token}");
        await host.FlushAsync("GET /hubs/orders/negotiate");

        var span = host.Spans.Should().ContainSingle().Subject;
        span.TagObjects.Select(tag => tag.Value?.ToString()).Should().NotContain(value => value!.Contains(token));
        span.GetTagItem("url.query")?.ToString().Should().Contain("access_token=Redacted");
    }

    [Test]
    public async Task Consulta_ao_banco_sem_pai_nao_vai_para_o_trace()
    {
        using var npgsql = new ActivitySource(DropRootDatabaseSpansProcessor.NpgsqlSource);
        using var service = new ActivitySource("Test.Service");
        await using var host = await TelemetryHost.StartAsync(sources: "Test.Service");

        using (npgsql.StartActivity("SELECT polling", ActivityKind.Client)) { }
        using (service.StartActivity("rodada com trabalho"))
        using (npgsql.StartActivity("UPDATE pedido", ActivityKind.Client)) { }
        await host.FlushAsync();

        host.Spans.Select(span => span.DisplayName).Should().BeEquivalentTo("UPDATE pedido", "rodada com trabalho");
    }

    private static IEnumerable<string> RegisteredOtlpServices(TelemetryHost host) =>
        host.RegisteredTypes.Where(name => name.Contains("Otlp"));
}
