namespace Frameworker.Observability;

// Telemetria das APIs (add-service-telemetry): traces, métricas e logs em OpenTelemetry, enviados por OTLP a um
// coletor (no compose, o painel local). Sem OTEL_EXPORTER_OTLP_ENDPOINT, nada é exportado e a API funciona como antes.
public static class ObservabilityExtensions
{
    public const string EndpointVariable = "OTEL_EXPORTER_OTLP_ENDPOINT";

    // Health checks não geram traces (nem os spans das chamadas que eles fazem, filhos de um span descartado).
    private const string HealthPath = "/health";

    // tracing: fontes e instrumentações próprias da API (ex.: a do Hot Chocolate).
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder, string serviceName,
        Action<TracerProviderBuilder>? tracing = null)
    {
        var version = ServiceVersion();
        var environment = builder.Environment.EnvironmentName;

        var telemetry = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: version)
                // O nome atual da convenção e o antigo, que muitos coletores ainda leem como o ambiente.
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment.name"] = environment,
                    ["deployment.environment"] = environment
                })
                // Depois do AddService, para OTEL_SERVICE_NAME e OTEL_RESOURCE_ATTRIBUTES sobrescreverem.
                .AddEnvironmentVariableDetector())
            .WithTracing(traces => traces
                .AddSource("MassTransit")
                // A instrumentação já mascara os valores da query (ex.: o access_token do hub) e não grava cabeçalhos.
                .AddAspNetCoreInstrumentation(options =>
                    options.Filter = context => !context.Request.Path.StartsWithSegments(HealthPath))
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                // Antes do exportador: descarta as consultas de polling (spans raiz do Npgsql).
                .AddProcessor(new DropRootDatabaseSpansProcessor())
                // O span HTTP de uma requisição GraphQL leva o nome da operação.
                .AddProcessor(new GraphQLOperationNameProcessor())
                .With(tracing))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithLogging(logging => { }, options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = false;
            });

        if (IsExportEnabled(builder.Configuration))
            telemetry.UseOtlpExporter();

        return builder;
    }

    private static TracerProviderBuilder With(this TracerProviderBuilder traces, Action<TracerProviderBuilder>? tracing)
    {
        tracing?.Invoke(traces);
        return traces;
    }

    public static bool IsExportEnabled(IConfiguration configuration) =>
        !string.IsNullOrWhiteSpace(configuration[EndpointVariable]);

    private static string ServiceVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        var version = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                      ?? assembly?.GetName().Version?.ToString()
                      ?? "0.0.0";
        // Sem o hash do commit que o SDK acrescenta depois do "+".
        return version.Split('+')[0];
    }
}
