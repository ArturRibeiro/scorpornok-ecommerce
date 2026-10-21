namespace Payments.Tests.CardTokens;

// O cartão chega ao subgraph no documento ou nas variáveis da tokenizeCard: nada disso pode ir para os spans
// (add-service-telemetry). Mesma instrumentação do Program.cs.
[TestFixture]
public class CardTokenTelemetryTests
{
    private const string CardNumber = "4111111111111111";
    private const string Cvv = "8642";

    private const string WithVariables =
        "mutation Tokenize($input: TokenizeCardInput!) { tokenizeCard(input: $input) { token { token } errors { field message } } }";

    private static readonly string Literal =
        "mutation Tokenize { tokenizeCard(input: { cardHolderName: \"Fulano de Tal\", cardNumber: \"" + CardNumber +
        "\", expirationMonth: \"12\", expirationYear: \"2030\", cvv: \"" + Cvv + "\" }) { token { token } errors { field message } } }";

    private static readonly string Variables = JsonSerializer.Serialize(new
    {
        input = new { cardHolderName = "Fulano de Tal", cardNumber = CardNumber, expirationMonth = "12",
            expirationYear = "2030", cvv = Cvv }
    });

    [TestCase(true, false)]
    [TestCase(false, false)]
    [TestCase(true, true)]
    [TestCase(false, true)]
    public async Task Cartao_nao_aparece_nos_spans(bool literal, bool fails)
    {
        var bus = new Mock<IMemoryBus>();
        var setup = bus.Setup(b => b.RequestAsync(It.IsAny<Message<CreateCardTokenResult>>()));
        if (fails)
            setup.ThrowsAsync(new InvalidOperationException("Falha ao gravar o token."));
        else
            setup.ReturnsAsync(new CreateCardTokenResult("tok_abc", "1111", DateTime.UtcNow, new Dictionary<string, string>()));

        var spans = new ExportedItems<Activity>();
        await using (var app = await StartAsync(bus.Object, spans))
        {
            var body = literal ? new { query = Literal, variables = (object?)null }
                : new { query = WithVariables, variables = (object?)JsonDocument.Parse(Variables).RootElement };
            var response = await app.GetTestClient().PostAsJsonAsync("/graphql", body);
            (await response.Content.ReadAsStringAsync()).Should().NotContain(CardNumber);
            // O ASP.NET Core encerra o span da requisição logo depois de responder: o cliente pode chegar antes dele.
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (!spans.Any(span => span.Kind == ActivityKind.Server) && DateTime.UtcNow < deadline)
                await Task.Delay(20);
            app.Services.GetRequiredService<TracerProvider>().ForceFlush();
        }

        spans.Should().Contain(span => span.Kind == ActivityKind.Server);
        spans.Select(span => span.DisplayName).Should().Contain("mutation Tokenize");
        var exported = string.Join('\n', spans.Select(Describe));
        exported.Should().NotContain(CardNumber).And.NotContain(Cvv);
    }

    // Subgraph em HTTP, como no Program.cs: o span da requisição é o que pode levar o documento e as variáveis.
    private static async Task<WebApplication> StartAsync(IMemoryBus bus, ExportedItems<Activity> spans)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.AddObservability(Program.ServiceName, tracing => tracing.AddHotChocolateInstrumentation());
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddInMemoryExporter(spans));
        builder.Services.AddSingleton(bus);
        builder.Services.AddGraphQLServer(Program.SubgraphName)
            .ModifyOptions(o => o.StrictValidation = false)
            .AddApiTypes()
            .AddInstrumentation(options => options.WithoutSensitiveData());
        var app = builder.Build();
        app.MapGraphQL(schemaName: Program.SubgraphName);
        await app.StartAsync();
        return app;
    }

    // Tudo o que o exportador manda de um span: nome, tags, eventos (com as tags) e status.
    private static string Describe(Activity span) => string.Join('|',
        new[] { span.DisplayName, span.StatusDescription ?? "" }
            .Concat(span.TagObjects.Select(tag => $"{tag.Key}={tag.Value}"))
            .Concat(span.Events.SelectMany(e => e.Tags.Select(tag => $"{e.Name}:{tag.Key}={tag.Value}"))));
}
