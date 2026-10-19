var builder = WebApplication.CreateBuilder(args);

// Vem do appsettings.Development.json (dotnet run) ou do docker compose (variável de ambiente).
var connectionString = builder.Configuration.GetConnectionString("ConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:ConnectionString não configurada.");

// A página "Meu cadastro" e o checkout do front-end (src/frontend) chamam a API direto do navegador
// (Cors:AllowedOrigins no appsettings.json). O token vai no cabeçalho, sem cookies: sem AllowCredentials.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(connectionString, builder.Configuration);
// Clientes do Keycloak: o cadastro exige token com e-mail confirmado.
builder.Services.AddCustomerAuthentication(builder.Configuration, builder.Environment);
// Subgraph do gateway federado (Fusion): o cadastro em /graphql, com a mesma política do REST.
builder.Services.AddGraphQLServer(SubgraphName).AddAuthorization().AddApiTypes();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<CustomerContext>(tags: ["ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.GetCustomerProfile();
app.UpdateCustomerProfile();
app.MapGraphQL(schemaName: SubgraphName);
// live: o processo responde (não executa checks); ready: pode ler e salvar cadastros (banco);
// bus: conexão com o RabbitMQ (OrderPlaced), fora do ready porque o cadastro não depende dela.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });
app.MapHealthChecks("/health/bus", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("bus"),
    // Broker que cai depois da partida deixa o bus "Degraded" (reconectando); o padrão responderia 200.
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

// "schema export" (scripts/compose-gateway.sh) só gera o schema: não precisa do banco nem da seed.
if (args is ["schema", ..])
{
    await app.RunWithGraphQLCommandsAsync(args);
    return;
}

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<CustomerContext>().Seed();

app.Run();

public partial class Program
{
    // Nome do subgraph no gateway (name do schema-settings.json).
    public const string SubgraphName = "customers";
}
