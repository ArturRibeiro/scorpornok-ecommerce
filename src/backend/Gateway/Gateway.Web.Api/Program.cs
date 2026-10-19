using Gateway.Web.Api.Authentication;
using Gateway.Web.Api.GraphQL;
using Gateway.Web.Api.Health;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// O schema composto vem do .far gerado por scripts/compose-gateway.sh: gateway.local.far (dotnet run, Catalog
// em localhost:5064) ou gateway.compose.far (compose, catalog-api:8080), escolhido em Fusion:Archive.
var archive = Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["Fusion:Archive"] ?? "gateway.local.far");
var limits = GatewayLimits.From(builder.Configuration);

// A loja chama o gateway direto do navegador (Cors:AllowedOrigins); os subgraphs, só o gateway os chama.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

// Token do Keycloak (audience gateway-api), validado aqui e repassado aos subgraphs.
builder.Services.AddGatewayAuthentication(builder.Configuration, builder.Environment);

// Cliente HTTP dos subgraphs: uma API que não responde em 15 s vira erro só no campo dela (o placeOrder
// consulta o Catalog com limite próprio de 5 s); o token da loja
// segue em cada chamada.
builder.Services.AddHttpClient("fusion", client => client.Timeout = TimeSpan.FromSeconds(15))
    .AddHttpMessageHandler<AuthorizationForwardingHandler>();

// A política padrão do Fusion já desliga a introspecção fora de Development.
builder.AddGraphQLGateway()
    .AddFileSystemConfiguration(archive)
    .AddMaxExecutionDepthRule(limits.MaxDepth, skipIntrospectionFields: true)
    .AddValidationRule((_, _) => new MaxFieldsRule(limits.MaxFields));

builder.Services.AddHealthChecks()
    .AddCheck<FusionSchemaHealthCheck>("fusion-schema", tags: ["ready"]);

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.RejectInvalidCredentials();
app.MapGraphQL();
// live: o processo responde (não executa checks); ready: o schema composto está carregado.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();

public partial class Program;
