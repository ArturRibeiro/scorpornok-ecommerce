var builder = WebApplication.CreateBuilder(args);

// Vem do appsettings.Development.json (dotnet run) ou do docker compose (variável de ambiente).
var connectionString = builder.Configuration.GetConnectionString("ConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:ConnectionString não configurada.");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Banco, gateway e o consumidor das solicitações de pagamento (RabbitMq no appsettings).
builder.Services.AddInfrastructure(connectionString, builder.Configuration);
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentContext>(tags: ["ready"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// live: o processo responde (não executa checks); ready: o banco responde (o broker não entra no check).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

using (var scope = app.Services.CreateScope())
    await scope.ServiceProvider.GetRequiredService<PaymentContext>().Database.EnsureCreatedAsync();
Console.WriteLine("Database created successfully!");

app.Run();

public partial class Program
{
}
