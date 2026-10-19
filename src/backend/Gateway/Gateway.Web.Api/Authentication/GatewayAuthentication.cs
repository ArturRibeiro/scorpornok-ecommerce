using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Gateway.Web.Api.Authentication;

/// <summary>
/// O gateway valida o token do Keycloak (seção <c>Keycloak</c>, audience <c>gateway-api</c>) e o repassa
/// aos subgraphs, que validam de novo com o seu audience e aplicam a regra de acesso (<c>VerifiedCustomer</c>).
/// Sem token, a consulta segue: o catálogo é público, e o subgraph decide o que exige login.
/// </summary>
public static class GatewayAuthentication
{
    public static IServiceCollection AddGatewayAuthentication(this IServiceCollection services,
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
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters.ValidIssuer = authority;
                options.TokenValidationParameters.ValidAudience = audience;
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
            });
        services.AddHttpContextAccessor();
        services.AddTransient<AuthorizationForwardingHandler>();
        return services;
    }

    /// <summary>
    /// Token presente e recusado (inválido, expirado ou de outro audience) → 401, sem chegar aos subgraphs:
    /// a loja pede o login de novo. Sem token, segue.
    /// </summary>
    public static IApplicationBuilder RejectInvalidCredentials(this IApplicationBuilder app)
        => app.Use(async (context, next) =>
        {
            if (context.Request.Headers.Authorization.Count > 0)
            {
                var result = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                if (!result.Succeeded)
                {
                    await context.ChallengeAsync(JwtBearerDefaults.AuthenticationScheme);
                    return;
                }
            }
            await next();
        });
}

/// <summary>Repassa o <c>Authorization</c> da requisição da loja em cada chamada aos subgraphs.</summary>
public sealed class AuthorizationForwardingHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = accessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authorization) && !request.Headers.Contains("Authorization"))
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
        return base.SendAsync(request, cancellationToken);
    }
}
