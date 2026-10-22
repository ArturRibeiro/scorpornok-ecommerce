namespace Gateway.Tests;

// O cartão passa pelo gateway (tokenizeCard). Ele não pode aparecer em log nem na resposta, nem com erro.
[TestFixture]
public class CardDataTests
{
    private const string CardNumber = "4111111111111111";
    private const string Cvv = "8642";

    private static string Mutation =>
        "mutation { tokenizeCard(input: { cardHolderName: \"Fulano de Tal\", cardNumber: \"" + CardNumber +
        "\", expirationMonth: \"12\", expirationYear: \"2030\", cvv: \"" + Cvv + "\" }) { token { token } errors { field message } } }";

    // Como a loja manda: o cartão nas variáveis.
    private const string MutationWithVariables =
        "mutation Tokenize($input: TokenizeCardInput!) { tokenizeCard(input: $input) { token { token } errors { field message } } }";

    private static readonly object Variables = new
    {
        input = new { cardHolderName = "Fulano de Tal", cardNumber = CardNumber, expirationMonth = "12",
            expirationYear = "2030", cvv = Cvv }
    };

    [Test]
    public async Task Tokenizacao_vai_ao_Payments()
    {
        await using var gateway = new GatewayFactory();
        gateway.Subgraphs.Respond = """{"data":{"tokenizeCard":{"token":{"token":"tok_1"},"errors":[]}}}""";

        var response = await gateway.QueryAsync(Mutation);

        response.Status.Should().Be(HttpStatusCode.OK);
        response.Body.Should().Contain("tok_1");
    }

    // Spans e logs que iriam para o coletor (add-service-telemetry), com sucesso e com o Payments
    // fora, com o cartão no documento e nas variáveis.
    [TestCase(false, false)]
    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(true, true)]
    public async Task Cartao_nao_vai_para_a_telemetria(bool withVariables, bool paymentsDown)
    {
        var telemetry = new CapturedTelemetry();
        await using var gateway = new GatewayFactory(telemetry: telemetry);
        gateway.Subgraphs.Respond = """{"data":{"tokenizeCard":{"token":{"token":"tok_1"},"errors":[]}}}""";
        gateway.Subgraphs.Down = paymentsDown;

        var response = withVariables
            ? await gateway.QueryAsync(MutationWithVariables, variables: Variables)
            : await gateway.QueryAsync(Mutation);
        var exported = await telemetry.FlushAsync(gateway.Services);

        response.Body.Should().Contain(paymentsDown ? "errors" : "tok_1");
        gateway.Subgraphs.Calls.Should().Be(1);
        // O span da requisição leva o nome da operação (a do documento literal não tem nome).
        telemetry.Spans.Single(span => span.Kind == ActivityKind.Server).DisplayName
            .Should().Be(withVariables ? "mutation Tokenize" : "mutation");
        telemetry.Logs.Should().NotBeEmpty();
        exported.Should().NotContain(CardNumber).And.NotContain(Cvv);
    }

    // Development é o pior caso (o padrão do Hot Chocolate mostraria detalhes da exceção).
    [TestCase("Development")]
    [TestCase("Production")]
    public async Task Com_o_Payments_fora_o_cartao_nao_vaza_em_log_nem_na_resposta(string environment)
    {
        var logs = new CapturedLogs();
        await using var gateway = new GatewayFactory(environment, logs: logs);
        gateway.Subgraphs.Down = true;

        var response = await gateway.QueryAsync(Mutation);

        response.Body.Should().Contain("errors");
        response.Body.Should().NotContain(CardNumber).And.NotContain(Cvv);
        logs.All.Should().NotBeEmpty();
        logs.All.Should().NotContain(CardNumber).And.NotContain(Cvv);
    }
}
