using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Orders.Web.Api.Hubs;

namespace Orders.Web.Api.Authentication;

public static class CustomerAuthentication
{
    /// <summary>Cliente autenticado, com e-mail confirmado e identidade completa.</summary>
    public const string VerifiedCustomerPolicy = "VerifiedCustomer";

    public static AuthorizationPolicy VerifiedCustomer { get; } = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim("email_verified", "true")
        .RequireAssertion(context => CustomerIdentity.From(context.User) is not null)
        .Build();

    /// <summary>
    /// Valida os tokens do Keycloak (seção <c>Keycloak</c>): sem token → 401; token válido que
    /// falha a política → 403. <c>Authority</c> é o emissor público (o mesmo que o navegador vê);
    /// <c>MetadataAddress</c>, quando presente, é de onde a API busca a descoberta e as chaves
    /// (no compose, <c>keycloak:8080</c>).
    /// </summary>
    public static IServiceCollection AddCustomerAuthentication(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        var keycloak = configuration.GetSection("Keycloak");
        var authority = keycloak["Authority"]
            ?? throw new InvalidOperationException("Keycloak:Authority não configurado.");
        var audience = keycloak["Audience"]
            ?? throw new InvalidOperationException("Keycloak:Audience não configurado.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                if (keycloak["MetadataAddress"] is { Length: > 0 } metadataAddress)
                    options.MetadataAddress = metadataAddress;
                options.Audience = audience;
                // Mantém "sub" e "email" com esses nomes, sem o mapeamento para os tipos do WS-Fed.
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters.ValidIssuer = authority;
                options.TokenValidationParameters.ValidAudience = audience;
                // O padrão (5 min) é a validade inteira do access token do Keycloak.
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
                // O WebSocket não leva o cabeçalho Authorization: no hub, o SignalR manda o token
                // em access_token na query. Só nesse caminho, para não aceitá-lo nas outras rotas.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token) && context.HttpContext.Request.Path.StartsWithSegments(OrderHub.Path))
                            context.Token = token;
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization(options => options.AddPolicy(VerifiedCustomerPolicy, VerifiedCustomer));
        return services;
    }
}
