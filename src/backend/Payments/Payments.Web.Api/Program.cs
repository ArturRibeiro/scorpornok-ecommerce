var builder = WebApplication.CreateBuilder(args);

// Traces, métricas e logs em OpenTelemetry; exportados só com OTEL_EXPORTER_OTLP_ENDPOINT (ex.: o painel local).
builder.AddObservability(ServiceName, tracing => tracing.AddHotChocolateInstrumentation());

// Vem do appsettings.Development.json (dotnet run) ou do docker compose (variável de ambiente).
var connectionString = builder.Configuration.GetConnectionString("ConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:ConnectionString não configurada.");

// O checkout tokeniza o cartão direto do navegador, então a origem da loja precisa estar
// liberada (Cors:AllowedOrigins no appsettings.json), como no Catalog e no Orders.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Banco, gateway e o consumidor das solicitações de pagamento (RabbitMq no appsettings).
builder.Services.AddInfrastructure(connectionString, builder.Configuration);
// Subgraph do gateway federado (Fusion): a tokenização do cartão em /graphql. O subgraph só tem mutation; sem
// a validação estrita, o schema não precisa de um tipo Query (as consultas vêm dos outros subgraphs).
builder.Services.AddGraphQLServer(SubgraphName).ModifyOptions(o => o.StrictValidation = false).AddApiTypes()
    .AddInstrumentation(options => options.WithoutSensitiveData());
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentContext>(tags: ["ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Sem UseHttpsRedirection: a API só é servida por HTTP (compose e perfil http), e redirecionar quebraria o CORS.
app.UseCors();

app.CreateCardToken();
app.MapGraphQL(schemaName: SubgraphName);

// live: o processo responde (não executa checks); ready: o banco responde (o broker não entra no check).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
// ops: o que a operação precisa acompanhar (filas de erro), fora do ready: o problema não
// impede a API de atender, mas exige ação. Unhealthy responde 503.
app.MapHealthChecks("/health/ops", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ops"),
    // Mostra o que cada check encontrou (ex.: "payment-approved_error=1"), não só o status geral.
    ResponseWriter = (context, report) => context.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        checks = report.Entries.ToDictionary(e => e.Key,
            e => new { status = e.Value.Status.ToString(), description = e.Value.Description })
    })
});

// "schema export" (scripts/compose-gateway.sh) só gera o schema: não precisa do banco.
if (args is ["schema", ..])
{
    await app.RunWithGraphQLCommandsAsync(args);
    return;
}

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<PaymentContext>().Database.EnsureCreatedAsync();
Console.WriteLine("Database created successfully!");

app.Run();

public partial class Program
{
    // Nome do serviço na telemetria (service.name), o mesmo do compose.
    public const string ServiceName = "payment-api";

    // Nome do subgraph no gateway (name do schema-settings.json).
    public const string SubgraphName = "payments";
}
