using System.Text.Json;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Payments.CommandHandlers.CardTokens;
using Payments.Web.Api.GraphQL;
using Shared.Code;
using Shared.Code.Commands;

namespace Payments.Tests.CardTokens;

// A tokenização no GraphQL (subgraph do gateway): o mesmo handler do POST /card-tokens, com os erros por campo.
[TestFixture]
public class CardTokenGraphTests
{
    private const string Mutation =
        "mutation($input: TokenizeCardInput!) { tokenizeCard(input: $input) { token { token last4 expiresAt } errors { field message } } }";

    private static readonly object Card = new
    {
        input = new { cardHolderName = "Fulano de Tal", cardNumber = "4111111111111111", expirationMonth = "12",
            expirationYear = "2030", cvv = "123" }
    };

    private Mock<IMemoryBus> _bus = null!;
    private CreateCardTokenCommand? _sent;

    [SetUp]
    public void SetUp()
    {
        _sent = null;
        _bus = new Mock<IMemoryBus>();
    }

    private void Returns(CreateCardTokenResult result)
        => _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CreateCardTokenResult>>()))
            .Callback((Message<CreateCardTokenResult> c) => _sent = (CreateCardTokenCommand)c)
            .ReturnsAsync(result);

    private async Task<JsonElement> Execute()
    {
        var executor = await new ServiceCollection()
            .AddSingleton(_bus.Object)
            .AddGraphQLServer(Program.SubgraphName)
            .ModifyOptions(o => o.StrictValidation = false)
            .AddApiTypes()
            .BuildRequestExecutorAsync(Program.SubgraphName);
        var result = await executor.ExecuteAsync(OperationRequestBuilder.New().SetDocument(Mutation)
            .SetVariableValues(JsonSerializer.Serialize(Card)).Build());
        return JsonDocument.Parse(result.ToJson()).RootElement.GetProperty("data").GetProperty("tokenizeCard");
    }

    [Test]
    public async Task Cartao_valido_devolve_o_token_e_os_4_ultimos_digitos()
    {
        Returns(new CreateCardTokenResult("tok_abc", "1111", new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc),
            new Dictionary<string, string>()));

        var payload = await Execute();

        payload.GetProperty("token").GetProperty("token").GetString().Should().Be("tok_abc");
        payload.GetProperty("token").GetProperty("last4").GetString().Should().Be("1111");
        payload.GetProperty("errors").GetArrayLength().Should().Be(0);
        _sent!.CardNumber.Should().Be("4111111111111111");
        _sent.Cvv.Should().Be("123");
    }

    [Test]
    public async Task Erros_voltam_por_campo_sem_token()
    {
        Returns(CreateCardTokenResult.Invalid(new Dictionary<string, string> { ["cvv"] = "CVV inválido." }));

        var payload = await Execute();

        payload.GetProperty("token").ValueKind.Should().Be(JsonValueKind.Null);
        var error = payload.GetProperty("errors")[0];
        error.GetProperty("field").GetString().Should().Be("cvv");
        error.GetProperty("message").GetString().Should().Be("CVV inválido.");
    }

    [Test]
    public void Input_nao_mostra_o_cartao_ao_virar_texto()
        => new TokenizeCardInput("Fulano", "4111111111111111", "12", "2030", "123").ToString()
            .Should().NotContain("4111").And.NotContain("123");
}
