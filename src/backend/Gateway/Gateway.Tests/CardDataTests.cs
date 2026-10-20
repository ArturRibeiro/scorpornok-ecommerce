using System.Net;

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

    [Test]
    public async Task Tokenizacao_vai_ao_Payments()
    {
        await using var gateway = new GatewayFactory();
        gateway.Subgraphs.Respond = """{"data":{"tokenizeCard":{"token":{"token":"tok_1"},"errors":[]}}}""";

        var response = await gateway.QueryAsync(Mutation);

        response.Status.Should().Be(HttpStatusCode.OK);
        response.Body.Should().Contain("tok_1");
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
