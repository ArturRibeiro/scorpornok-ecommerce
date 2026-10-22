namespace Gateway.Tests;

// O gateway valida o token (audience gateway-api) e o repassa; quem decide o acesso é o subgraph.
[TestFixture]
public class AuthenticationTests
{
    private const string Products = "{ products { items { id } } }";

    [Test]
    public async Task Token_valido_e_repassado_ao_subgraph()
    {
        await using var gateway = new GatewayFactory();
        var token = TestTokens.Create();

        var response = await gateway.QueryAsync(Products, token);

        response.Status.Should().Be(HttpStatusCode.OK);
        gateway.Subgraphs.LastAuthorization.Should().Be($"Bearer {token}");
    }

    [Test]
    public async Task Sem_token_o_catalogo_responde_e_nada_e_repassado()
    {
        await using var gateway = new GatewayFactory();

        var response = await gateway.QueryAsync(Products);

        response.Status.Should().Be(HttpStatusCode.OK);
        gateway.Subgraphs.Calls.Should().Be(1);
        gateway.Subgraphs.LastAuthorization.Should().BeNull();
    }

    [TestCase("invalido")]
    [TestCase("outro-audience")]
    [TestCase("expirado")]
    public async Task Token_recusado_responde_401_sem_chamar_o_subgraph(string kind)
    {
        await using var gateway = new GatewayFactory();
        var token = kind switch
        {
            "outro-audience" => TestTokens.Create(audience: "orders-api"),
            "expirado" => TestTokens.Create(expires: DateTime.UtcNow.AddMinutes(-5)),
            _ => "nao-e-um-jwt"
        };

        var response = await gateway.QueryAsync(Products, token);

        response.Status.Should().Be(HttpStatusCode.Unauthorized);
        gateway.Subgraphs.Calls.Should().Be(0);
    }
}
