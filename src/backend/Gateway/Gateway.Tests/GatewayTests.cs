namespace Gateway.Tests;

[TestFixture]
public class GatewayTests
{
    private const string Products = "{ products { items { id } } }";
    private const string Introspection = "{ __schema { queryType { name } } }";

    [Test]
    public async Task Consulta_vai_ao_Catalog_e_volta()
    {
        await using var gateway = new GatewayFactory();

        var response = await gateway.QueryAsync(Products);

        response.Status.Should().Be(HttpStatusCode.OK);
        response.Json.GetProperty("data").GetProperty("products").GetProperty("items")[0].GetProperty("id").GetInt64()
            .Should().Be(1);
        gateway.Subgraphs.Calls.Should().Be(1);
    }

    [TestCase("http://localhost:3000", true)]
    [TestCase("http://evil.example", false)]
    public async Task So_a_origem_da_loja_e_aceita(string origin, bool allowed)
    {
        await using var gateway = new GatewayFactory();
        using var client = gateway.CreateClient();
        using var preflight = new HttpRequestMessage(HttpMethod.Options, "/graphql");
        preflight.Headers.Add("Origin", origin);
        preflight.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(preflight);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().Be(allowed);
    }

    [Test]
    public async Task Consulta_mais_profunda_que_o_limite_e_recusada_sem_chamar_a_API()
    {
        // products.items.id tem profundidade 3.
        await using var gateway = new GatewayFactory(settings: new Dictionary<string, string?> { ["GraphQL:MaxDepth"] = "2" });

        var response = await gateway.QueryAsync(Products);

        response.Body.Should().Contain("errors").And.Contain("depth");
        response.Json.TryGetProperty("data", out _).Should().BeFalse();
        gateway.Subgraphs.Calls.Should().Be(0);
    }

    [Test]
    public async Task Consulta_com_campos_demais_e_recusada_sem_chamar_a_API()
    {
        await using var gateway = new GatewayFactory(settings: new Dictionary<string, string?> { ["GraphQL:MaxFields"] = "10" });
        var aliases = string.Join(' ', Enumerable.Range(0, 5).Select(i => $"a{i}: products {{ items {{ id }} }}"));

        var response = await gateway.QueryAsync($"{{ {aliases} }}");

        response.Body.Should().Contain(MaxFieldsRule.ErrorCode);
        gateway.Subgraphs.Calls.Should().Be(0);
    }

    [Test]
    public async Task Fragmento_repetido_conta_a_cada_uso()
    {
        await using var gateway = new GatewayFactory(settings: new Dictionary<string, string?> { ["GraphQL:MaxFields"] = "10" });
        var spreads = string.Join(' ', Enumerable.Range(0, 5).Select(i => $"a{i}: productById(id: 1) {{ ...F }}"));

        var response = await gateway.QueryAsync($"{{ {spreads} }} fragment F on Product {{ id name }}");

        response.Body.Should().Contain(MaxFieldsRule.ErrorCode);
        gateway.Subgraphs.Calls.Should().Be(0);
    }

    [Test]
    public async Task Introspeccao_fora_de_Development_e_recusada()
    {
        await using var gateway = new GatewayFactory(environment: "Production");

        var response = await gateway.QueryAsync(Introspection);

        response.Body.Should().Contain("Introspection is not allowed");
    }

    [Test]
    public async Task Introspeccao_em_Development_devolve_o_schema()
    {
        await using var gateway = new GatewayFactory();

        var response = await gateway.QueryAsync(Introspection);

        response.Json.GetProperty("data").GetProperty("__schema").GetProperty("queryType").GetProperty("name")
            .GetString().Should().Be("Query");
    }

    [Test]
    public async Task Catalog_fora_vira_erro_no_campo_e_o_gateway_continua_no_ar()
    {
        await using var gateway = new GatewayFactory();
        gateway.Subgraphs.Down = true;

        var response = await gateway.QueryAsync(Products);

        response.Status.Should().Be(HttpStatusCode.OK);
        var error = response.Json.GetProperty("errors")[0];
        error.GetProperty("path")[0].GetString().Should().Be("products");
        using var client = gateway.CreateClient();
        (await client.GetAsync("/health/live")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Pronto_com_o_Catalog_fora()
    {
        await using var gateway = new GatewayFactory();
        gateway.Subgraphs.Down = true;
        using var client = gateway.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        gateway.Subgraphs.Calls.Should().Be(0);
    }

    [Test]
    public void Limites_nao_positivos_impedem_a_partida()
    {
        var factory = new GatewayFactory(settings: new Dictionary<string, string?> { ["GraphQL:MaxDepth"] = "0" });

        var start = () => factory.CreateClient();

        start.Should().Throw<InvalidOperationException>().WithMessage("*MaxDepth*");
    }
}
