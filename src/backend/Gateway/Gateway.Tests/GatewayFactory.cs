using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Gateway.Tests;

/// <summary>
/// Gateway em memória, com o gateway.local.far. As chamadas aos subgraphs passam pelo <see cref="Subgraphs"/>,
/// que as conta e responde no lugar do Catalog (ou simula o Catalog fora do ar).
/// </summary>
public sealed class GatewayFactory(string environment = "Development", IDictionary<string, string?>? settings = null)
    : WebApplicationFactory<Program>
{
    public FakeSubgraphs Subgraphs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        foreach (var (key, value) in settings ?? new Dictionary<string, string?>())
            builder.UseSetting(key, value);
        builder.ConfigureTestServices(services =>
        {
            services.AddHttpClient("fusion").ConfigurePrimaryHttpMessageHandler(() => Subgraphs);
            // Sem buscar as chaves no Keycloak: os tokens dos testes (TestTokens) são assinados com uma chave local.
            // Configure, e não PostConfigure: roda antes da validação do JwtBearer (HTTPS fora de Development).
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.Configuration = new OpenIdConnectConfiguration { Issuer = TestTokens.Issuer };
                options.TokenValidationParameters.IssuerSigningKey = TestTokens.Key;
            });
        });
    }

    public async Task<GraphQLResponse> QueryAsync(string query, string? token = null)
    {
        using var client = CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.PostAsJsonAsync("/graphql", new { query });
        return new GraphQLResponse(response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}

public sealed record GraphQLResponse(HttpStatusCode Status, string Body)
{
    public System.Text.Json.JsonElement Json => System.Text.Json.JsonDocument.Parse(Body).RootElement;
}

/// <summary>Subgraphs falsos: conta as chamadas e devolve <see cref="Respond"/>, ou falha como API fora.</summary>
public sealed class FakeSubgraphs : HttpMessageHandler
{
    public int Calls { get; private set; }

    /// <summary>O <c>Authorization</c> da última chamada ao subgraph.</summary>
    public string? LastAuthorization { get; private set; }

    public bool Down { get; set; }

    public string Respond { get; set; } = """{"data":{"products":{"items":[{"id":1}]}}}""";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Calls++;
        LastAuthorization = request.Headers.Authorization?.ToString();
        if (Down)
            throw new HttpRequestException("Connection refused (catalog-api:8080)");
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(Respond, System.Text.Encoding.UTF8, "application/json")
        });
    }
}
