namespace Gateway.Tests;

// O cadastro vai ao subgraph do Customers com o token; quem aplica a regra de acesso é o subgraph.
[TestFixture]
public class CustomersRoutingTests
{
    [Test]
    public async Task Cadastro_vai_ao_Customers_com_o_token()
    {
        await using var gateway = new GatewayFactory();
        gateway.Subgraphs.Respond = """{"data":{"me":{"email":"cliente@exemplo.com"}}}""";
        var token = TestTokens.Create();

        var response = await gateway.QueryAsync("{ me { email } }", token);

        response.Status.Should().Be(HttpStatusCode.OK);
        response.Body.Should().Contain("cliente@exemplo.com");
        gateway.Subgraphs.LastAuthorization.Should().Be($"Bearer {token}");
    }

    [Test]
    public async Task Erro_de_acesso_do_subgraph_chega_a_loja()
    {
        await using var gateway = new GatewayFactory();
        gateway.Subgraphs.Respond = """{"errors":[{"message":"The current user is not authorized to access this resource.","path":["me"],"extensions":{"code":"AUTH_NOT_AUTHENTICATED"}}],"data":null}""";

        var response = await gateway.QueryAsync("{ me { email } }");

        response.Body.Should().Contain("AUTH_NOT_AUTHENTICATED");
        gateway.Subgraphs.Calls.Should().Be(1);
    }
}
