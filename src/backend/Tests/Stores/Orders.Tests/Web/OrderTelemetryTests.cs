using OpenTelemetry.Logs;

namespace Orders.Tests.Web;

// CPF, telefone e e-mail chegam no placeOrder (variáveis vindas do gateway ou escritos no documento) e no token:
// nada disso pode ir para os spans nem para os logs exportados (add-service-telemetry).
[TestFixture]
public class OrderTelemetryTests
{
    private const string Cpf = "52998224725";
    private const string Phone = "11999998888";
    private const string Email = "maria.souza@exemplo.com";

    private const string WithVariables =
        "mutation PlaceOrder($input: PlaceOrderInput!) { placeOrder(input: $input) { order { orderNumber } errors { code message } } }";

    private const string Literal =
        "mutation PlaceOrder { placeOrder(input: { address: { street: \"Rua A\", number: \"10\", district: \"Centro\", " +
        "city: \"São Paulo\", state: \"SP\", zipCode: \"01001-000\" }, customer: { fullName: \"Maria Souza\", phone: \"" +
        Phone + "\", cpf: \"" + Cpf + "\" }, items: [{ productId: 42, units: 2 }], payment: { cardToken: \"tok_1\", installments: 1 } }) " +
        "{ order { orderNumber } errors { code message } } }";

    private static readonly object Variables = new
    {
        input = new
        {
            address = new { street = "Rua A", number = "10", district = "Centro", city = "São Paulo", state = "SP", zipCode = "01001-000" },
            customer = new { fullName = "Maria Souza", phone = Phone, cpf = Cpf },
            items = new[] { new { productId = 42, units = 2 } },
            payment = new { cardToken = "tok_1", installments = 1 }
        }
    };

    [TestCase(false, CreateOrderOutcome.Created)]
    [TestCase(true, CreateOrderOutcome.Created)]
    [TestCase(false, CreateOrderOutcome.Invalid)]
    public async Task Dados_do_comprador_nao_aparecem_na_telemetria(bool literal, CreateOrderOutcome outcome)
    {
        var bus = new Mock<IMemoryBus>();
        bus.Setup(b => b.RequestAsync(It.IsAny<Message<CreateOrderResult>>()))
            .ReturnsAsync(outcome == CreateOrderOutcome.Created
                ? new CreateOrderResult(CreateOrderOutcome.Created, "A123456789012", "Pending", 20m, [])
                : CreateOrderResult.Invalid(["CPF inválido."]));
        var spans = new ExportedItems<Activity>();
        var logs = new ExportedItems<LogRecord>();
        string exported;

        await using (var app = await StartAsync(bus.Object, spans, logs))
        {
            var response = await app.GetTestClient().PostAsJsonAsync("/graphql",
                literal ? new { query = Literal, variables = (object?)null } : new { query = WithVariables, variables = (object?)Variables });
            (await response.Content.ReadAsStringAsync()).Should().Contain("placeOrder");
            await UntilRequestSpan(spans);
            app.Services.GetRequiredService<OpenTelemetry.Trace.TracerProvider>().ForceFlush();
            app.Services.GetRequiredService<LoggerProvider>().ForceFlush();
            // Os registros de log são reaproveitados depois do encerramento: lidos com o host no ar.
            logs.Should().NotBeEmpty();
            exported = string.Join('\n', logs.Select(Describe));
        }

        spans.Should().Contain(span => span.Kind == ActivityKind.Server);
        spans.Select(span => span.DisplayName).Should().Contain("mutation PlaceOrder");
        exported += '\n' + string.Join('\n', spans.Select(Describe));
        exported.Should().NotContain(Cpf).And.NotContain(Phone).And.NotContain(Email);
    }

    // Subgraph em HTTP, como no Program.cs, com o cliente do token já autenticado.
    private static async Task<WebApplication> StartAsync(IMemoryBus bus, ExportedItems<Activity> spans, ExportedItems<LogRecord> logs)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        // Tudo a partir de Debug, para o teste cobrir também os logs que o appsettings costuma esconder.
        builder.Configuration["Logging:LogLevel:Default"] = "Debug";
        builder.Configuration["Logging:LogLevel:Microsoft.AspNetCore"] = "Debug";
        builder.AddObservability(Program.ServiceName, tracing => tracing.AddHotChocolateInstrumentation());
        builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddInMemoryExporter(spans));
        builder.Services.ConfigureOpenTelemetryLoggerProvider(logging => logging.AddInMemoryExporter(logs));
        builder.Services.AddSingleton(bus);
        builder.Services.AddAuthorization(options => options.AddPolicy(CustomerAuthentication.VerifiedCustomerPolicy,
            CustomerAuthentication.VerifiedCustomer));
        builder.Services.AddGraphQLServer(Program.SubgraphName)
            .AddAuthorization()
            .AddApiTypes()
            .AddInstrumentation(options => options.WithoutSensitiveData());
        var app = builder.Build();
        app.Use((context, next) =>
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString()),
                new Claim("email", Email), new Claim("email_verified", "true")], "Bearer"));
            return next(context);
        });
        app.MapGraphQL(schemaName: Program.SubgraphName);
        await app.StartAsync();
        return app;
    }

    // O ASP.NET Core encerra o span da requisição logo depois de responder: o cliente pode chegar antes dele.
    private static async Task UntilRequestSpan(IEnumerable<Activity> spans)
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (!spans.Any(span => span.Kind == ActivityKind.Server) && DateTime.UtcNow < deadline)
            await Task.Delay(20);
    }

    private static string Describe(Activity span) => string.Join('|',
        new[] { span.DisplayName, span.StatusDescription ?? "" }
            .Concat(span.TagObjects.Select(tag => $"{tag.Key}={tag.Value}"))
            .Concat(span.Events.SelectMany(e => e.Tags.Select(tag => $"{e.Name}:{tag.Key}={tag.Value}"))));

    private static string Describe(LogRecord log) => string.Join('|',
        new[] { log.FormattedMessage ?? "", log.Body ?? "", log.Exception?.ToString() ?? "" }
            .Concat(log.Attributes?.Select(a => $"{a.Key}={a.Value}") ?? []));
}
