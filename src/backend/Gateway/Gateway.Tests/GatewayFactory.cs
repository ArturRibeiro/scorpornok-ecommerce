namespace Gateway.Tests;

/// <summary>
/// Gateway em memória, com o gateway.local.far. As chamadas aos subgraphs passam pelo <see cref="Subgraphs"/>,
/// que as conta e responde no lugar do Catalog (ou simula o Catalog fora do ar).
/// </summary>
public sealed class GatewayFactory(string environment = "Development", IDictionary<string, string?>? settings = null,
    CapturedLogs? logs = null, CapturedTelemetry? telemetry = null)
    : WebApplicationFactory<Program>
{
    public FakeSubgraphs Subgraphs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
            builder.UseSetting(key, value);
        if (logs is not null)
            // Filtro do próprio provedor: vale mais que os do appsettings (ex.: Microsoft.AspNetCore em Warning),
            // para a captura ver tudo o que qualquer categoria registraria.
            builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Trace).AddProvider(logs)
                .AddFilter<CapturedLogs>(category: null, level: LogLevel.Trace));
        if (telemetry is not null)
            // O provedor de logs do OpenTelemetry também vê todos os níveis, como o CapturedLogs.
            builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Trace)
                .AddFilter<OpenTelemetryLoggerProvider>(category: null, level: LogLevel.Trace));
        builder.ConfigureTestServices(services =>
        {
            if (telemetry is not null)
            {
                // Exportadores em memória: o que iria para o coletor.
                services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddInMemoryExporter(telemetry.Spans));
                services.ConfigureOpenTelemetryLoggerProvider(logging => logging.AddInMemoryExporter(telemetry.Logs));
            }
            services.AddHttpClient("fusion").ConfigurePrimaryHttpMessageHandler(() => Subgraphs);
            // Sem buscar as chaves no Keycloak: os tokens dos testes (TestTokens) são assinados com uma chave local.
            // Configure, e não PostConfigure: roda antes da validação do JwtBearer (HTTPS fora de Development).
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.Configuration = new OpenIdConnectConfiguration { Issuer = TestTokens.Issuer };
                options.TokenValidationParameters.IssuerSigningKey = TestTokens.Key;
            });
        });
    }

    public async Task<GraphQLResponse> QueryAsync(string query, string? token = null, object? variables = null)
    {
        using var client = CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync("/graphql", new { query, variables });
        return new GraphQLResponse(response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}

public sealed record GraphQLResponse(HttpStatusCode Status, string Body)
{
    public System.Text.Json.JsonElement Json => System.Text.Json.JsonDocument.Parse(Body).RootElement;
}

/// <summary>Subgraphs falsos: conta as chamadas e devolve <see cref="Respond"/>, ou falha como API fora.</summary>
public sealed class FakeSubgraphs : HttpMessageHandler
{
    public int Calls { get; private set; }

    /// <summary>O <c>Authorization</c> da última chamada ao subgraph.</summary>
    public string? LastAuthorization { get; private set; }

    public bool Down { get; set; }

    public string Respond { get; set; } = """{"data":{"products":{"items":[{"id":1}]}}}""";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastAuthorization = request.Headers.Authorization?.ToString();
        if (Down)
            throw new HttpRequestException("Connection refused (catalog-api:8080)");
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Respond, System.Text.Encoding.UTF8, "application/json")
        });
    }
}

/// <summary>Spans e logs que o gateway exportaria, com o texto de tudo o que vai para o exportador.</summary>
public sealed class CapturedTelemetry
{
    public ExportedItems<Activity> Spans { get; } = new();
    public ExportedItems<LogRecord> Logs { get; } = new();

    /// <summary>
    /// Espera o span da requisição HTTP (o ASP.NET Core o encerra logo depois de responder; é ele que levaria as
    /// variáveis), esvazia os lotes pendentes e devolve o texto de tudo o que foi exportado (nomes, tags, eventos,
    /// mensagens, atributos e exceções). Chamar com o gateway no ar: os registros de log são reaproveitados depois.
    /// </summary>
    public async Task<string> FlushAsync(IServiceProvider services)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!Spans.Any(span => span.Kind == ActivityKind.Server) && DateTime.UtcNow < deadline)
            await Task.Delay(20);
        services.GetRequiredService<TracerProvider>().ForceFlush();
        services.GetRequiredService<LoggerProvider>().ForceFlush();
        return string.Join('\n', Spans.Select(Describe).Concat(Logs.Select(Describe)));
    }

    private static string Describe(Activity span) => string.Join('|',
        new[] { span.DisplayName, span.StatusDescription ?? "" }
            .Concat(span.TagObjects.Select(tag => $"{tag.Key}={tag.Value}"))
            .Concat(span.Events.SelectMany(e => e.Tags.Select(tag => $"{e.Name}:{tag.Key}={tag.Value}"))));

    private static string Describe(LogRecord log) => string.Join('|',
        new[] { log.FormattedMessage ?? "", log.Body ?? "", log.Exception?.ToString() ?? "" }
            .Concat(log.Attributes?.Select(a => $"{a.Key}={a.Value}") ?? []));
}

/// <summary>Guarda tudo o que o gateway registra em log (mensagem e exceção), em qualquer nível.</summary>
public sealed class CapturedLogs : ILoggerProvider
{
    private readonly System.Collections.Concurrent.ConcurrentQueue<string> _entries = new();

    public string All => string.Join('\n', _entries);

    public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _entries);

    public void Dispose() { }

    private sealed class Logger(string category, System.Collections.Concurrent.ConcurrentQueue<string> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            entries.Enqueue($"{category} scope: {state}");
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => entries.Enqueue($"{category} [{logLevel}] {formatter(state, exception)} {exception}");
    }
}
