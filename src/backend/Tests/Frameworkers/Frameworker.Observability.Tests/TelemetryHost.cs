namespace Frameworker.Observability.Tests;

// API mínima com o AddObservability e um exportador em memória no lugar do OTLP.
public sealed class TelemetryHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    public const string GraphQLSourceName = "Test.GraphQL";
    private static readonly ActivitySource GraphQLSource = new(GraphQLSourceName);

    public ExportedItems<Activity> Spans { get; }
    public ExportedItems<LogRecord> Logs { get; }
    public HttpClient Client { get; }
    public IServiceProvider Services => _app.Services;
    // Tipos registrados no contêiner (serviço e implementação), para conferir se o exportador OTLP entrou.
    public IReadOnlyList<string> RegisteredTypes { get; }

    private TelemetryHost(WebApplication app, HttpClient client, ExportedItems<Activity> spans, ExportedItems<LogRecord> logs,
        IReadOnlyList<string> types)
    {
        Logs = logs;
        RegisteredTypes = types;
        _app = app;
        Client = client;
        Spans = spans;
    }

    public static async Task<TelemetryHost> StartAsync(Dictionary<string, string?>? settings = null,
        params string[] sources)
    {
        var spans = new ExportedItems<Activity>();
        var logs = new ExportedItems<LogRecord>();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(settings ?? []);
        builder.AddObservability("test-api", tracing => tracing.AddSource(sources).AddSource(GraphQLSourceName));
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddInMemoryExporter(spans));
        builder.Services.ConfigureOpenTelemetryLoggerProvider(logging => logging.AddInMemoryExporter(logs));

        var types = builder.Services
            .SelectMany(d => new[] { d.ServiceType, d.ImplementationType ?? d.ImplementationInstance?.GetType() })
            .Select(type => type?.FullName ?? "")
            .ToList();

        var app = builder.Build();
        app.MapGet("/ping", () => "pong");
        app.MapGet("/health/live", () => "ok");
        app.MapGet("/log", (ILogger<TelemetryHost> logger) =>
        {
            logger.LogInformation("Pedido {OrderNumber} registrado.", "A123");
            return "ok";
        });
        // Imita o HttpClient (um log por etapa da chamada, e um aviso na falha) e o aviso de licença do MediatR.
        app.MapGet("/http-and-license", (ILoggerFactory loggers) =>
        {
            var client = loggers.CreateLogger($"{ObservabilityExtensions.HttpClientCategory}.catalog.LogicalHandler");
            client.LogInformation("Start processing HTTP request GET http://catalog-api/");
            client.LogWarning("Falha na chamada HTTP");
            loggers.CreateLogger(ObservabilityExtensions.MediatRLicenseCategory).LogWarning("You do not have a valid license key");
            return "ok";
        });
        // Imita o EF Core: um log por consulta, e um aviso quando algo dá errado.
        app.MapGet("/sql", (ILoggerFactory loggers) =>
        {
            var logger = loggers.CreateLogger(ObservabilityExtensions.DatabaseCommandCategory);
            logger.LogInformation("Executed DbCommand (1ms) SELECT 1");
            logger.LogWarning("Falha no DbCommand");
            return "ok";
        });
        app.MapGet("/hubs/orders/negotiate", () => "ok");
        // Imita o span da operação que o Hot Chocolate abre dentro da requisição.
        app.MapPost("/graphql", (HttpRequest request) =>
        {
            using var operation = GraphQLSource.StartActivity("GraphQL Operation");
            operation?.SetTag("graphql.processing.type", "request");
            operation?.SetTag("graphql.operation.type", request.Query["type"].ToString());
            var name = request.Query["name"].ToString();
            if (name.Length > 0) operation?.SetTag("graphql.operation.name", name);
            return "ok";
        });
        await app.StartAsync();
        return new TelemetryHost(app, app.GetTestClient(), spans, logs, types);
    }

    // Espera o span da requisição terminar (o ASP.NET Core o encerra logo depois de responder, então o cliente
    // pode receber a resposta antes) e garante que tudo chegou aos exportadores.
    public async Task FlushAsync(string? untilSpan = null)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (untilSpan is not null && !Spans.Any(span => span.DisplayName == untilSpan) && DateTime.UtcNow < deadline)
            await Task.Delay(20);
        _app.Services.GetRequiredService<TracerProvider>().ForceFlush();
        _app.Services.GetRequiredService<LoggerProvider>().ForceFlush();
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
    }
}
