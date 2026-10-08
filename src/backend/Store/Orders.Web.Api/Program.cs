using Orders.Web.Api;
using Orders.Web.Api.WebApplicationExtensions;

var builder = WebApplication.CreateBuilder(args);

// Vem do appsettings.Development.json (dotnet run) ou do docker compose (variável de ambiente).
var connectionString = builder.Configuration.GetConnectionString("ConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:ConnectionString não configurada.");

// O checkout do front-end (src/frontend) envia o pedido direto do navegador, então a
// origem dele precisa estar liberada (Cors:AllowedOrigins no appsettings.json).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderContext>(tags: ["ready"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.CreateOrder();
// live: o processo responde (não executa checks); ready: dependências ok (banco).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

using (var scope = app.Services.CreateScope()) 
    await scope.ServiceProvider.GetRequiredService<OrderContext>().Seed();
Console.WriteLine("Database created successfully!");

app.Run();
