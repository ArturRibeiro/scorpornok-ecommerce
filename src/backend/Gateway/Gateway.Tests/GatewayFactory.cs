namespace Gateway.Tests;

/// <summary>
/// Gateway em memória, com o gateway.local.far. As chamadas aos subgraphs passam pelo <see cref="Subgraphs"/>,
/// que as conta e responde no lugar do Catalog (ou simula o Catalog fora do ar).
/// </summary>
public sealed class GatewayFactory(string environment = "Development", IDictionary<string, string?>? settings = null,
    CapturedLogs? logs = null)
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
        builder.ConfigureTestServices(services =>
        {
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

    public async Task<GraphQLResponse> QueryAsync(string query, string? token = null)
    {
        using var client = CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync("/graphql", new { query });
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
