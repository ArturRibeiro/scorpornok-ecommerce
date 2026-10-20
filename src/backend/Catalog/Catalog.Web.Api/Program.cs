var builder = WebApplication.CreateBuilder(args);

// Vem do appsettings.Development.json (dotnet run) ou do docker compose (variável de ambiente).
var connectionString = builder.Configuration.GetConnectionString("ConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:ConnectionString não configurada.");

// O front-end (src/frontend) chama a API direto do navegador, então a origem
// dele precisa estar liberada (Cors:AllowedOrigins no appsettings.json).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(connectionString);
// Subgraph do gateway federado (Fusion): as mesmas consultas do REST, em /graphql.
builder.Services.AddGraphQLServer(SubgraphName).AddApiTypes();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationCatalogDbContext>(tags: ["ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.GetAllProducts();
app.GetProductById();
app.GetProductsByIds();
app.MapGraphQL(schemaName: SubgraphName);
// live: o processo responde (não executa checks); ready: dependências ok (banco).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });


// "schema export" (scripts/compose-gateway.sh) só gera o schema: não precisa do banco nem da seed.
if (args is ["schema", ..])
{
    await app.RunWithGraphQLCommandsAsync(args);
    return;
}

using (var scope = app.Services.CreateScope()) 
    await scope.ServiceProvider.GetRequiredService<ApplicationCatalogDbContext>().Seed();
Console.WriteLine("Database created successfully!");


app.Run();

public partial class Program
{
    // Nome do subgraph no gateway (name do schema-settings.json).
    public const string SubgraphName = "catalog";
}