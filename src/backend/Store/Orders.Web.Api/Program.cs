var builder = WebApplication.CreateBuilder(args);

// Vem do appsettings.Development.json (dotnet run) ou do docker compose (variável de ambiente).
var connectionString = builder.Configuration.GetConnectionString("ConnectionString")
    ?? throw new InvalidOperationException("ConnectionStrings:ConnectionString não configurada.");

// O checkout do front-end (src/frontend) envia o pedido e acompanha o pagamento direto do
// navegador, então a origem dele precisa estar liberada (Cors:AllowedOrigins no appsettings.json).
// O SignalR exige AllowCredentials junto com WithOrigins.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(connectionString, builder.Configuration);
// Avisa o navegador do resultado do pagamento (hub /hubs/orders).
builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderPaymentNotifier, SignalROrderPaymentNotifier>();
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
// O navegador não aplica CORS ao handshake do WebSocket; a origem é checada aqui.
var webSocketOptions = new WebSocketOptions();
foreach (var origin in allowedOrigins) webSocketOptions.AllowedOrigins.Add(origin);
app.UseWebSockets(webSocketOptions);

app.CreateOrder();
app.MapHub<OrderHub>(OrderHub.Path);
// live: o processo responde (não executa checks); ready: dependências ok (banco).
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

using (var scope = app.Services.CreateScope()) 
    await scope.ServiceProvider.GetRequiredService<OrderContext>().Seed();
Console.WriteLine("Database created successfully!");

app.Run();
