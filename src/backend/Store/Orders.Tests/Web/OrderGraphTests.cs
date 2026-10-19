using System.Security.Claims;
using System.Text.Json;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.History;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Orders.Web.Api.Authentication;
using Orders.Web.Api.GraphQL;
using Shared.Code;

namespace Orders.Tests.Web;

// Os pedidos no GraphQL (subgraph do gateway): mesmos handlers do REST, com cada desfecho no seu código.
[TestFixture]
public class OrderGraphTests
{
    private static readonly Guid TokenCustomer = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private Mock<IMemoryBus> _bus = null!;
    private object? _sent;

    [SetUp]
    public void SetUp()
    {
        _sent = null;
        _bus = new Mock<IMemoryBus>();
    }

    private void Returns<T>(T result)
        => _bus.Setup(b => b.RequestAsync(It.IsAny<Message<T>>()))
            .Callback((Message<T> m) => _sent = m)
            .ReturnsAsync(result);

    private static ClaimsPrincipal Customer(string emailVerified = "true")
        => new(new ClaimsIdentity([new Claim("sub", TokenCustomer.ToString()), new Claim("email", "cliente@exemplo.com"),
            new Claim("email_verified", emailVerified)], "Bearer"));

    private async Task<JsonElement> Execute(string query, ClaimsPrincipal user, object? variables = null)
    {
        var executor = await new ServiceCollection()
            .AddLogging()
            .AddAuthorization(options => options.AddPolicy(CustomerAuthentication.VerifiedCustomerPolicy,
                CustomerAuthentication.VerifiedCustomer))
            .AddSingleton(_bus.Object)
            .AddGraphQLServer(Program.SubgraphName)
            .AddAuthorization()
            .AddApiTypes()
            .BuildRequestExecutorAsync(Program.SubgraphName);

        var request = OperationRequestBuilder.New().SetDocument(query).SetUser(user);
        if (variables is not null) request.SetVariableValues(JsonSerializer.Serialize(variables));
        return JsonDocument.Parse((await executor.ExecuteAsync(request.Build())).ToJson()).RootElement;
    }

    private const string PlaceOrder =
        "mutation($input: PlaceOrderInput!) { placeOrder(input: $input) { order { orderNumber status total attempt maxAttempts } errors { code message } } }";

    private const string Retry =
        "mutation($input: RetryOrderPaymentInput!) { retryOrderPayment(input: $input) { orderNumber attempt errors { code message } } }";

    private static readonly object OrderInput = new
    {
        input = new
        {
            address = new { street = "Rua A", number = "10", district = "Centro", city = "São Paulo", state = "SP", zipCode = "01001-000" },
            customer = new { fullName = "Maria Souza", phone = "11999998888", cpf = "52998224725" },
            items = new[] { new { productId = 42, units = 2 } },
            payment = new { cardToken = "tok_1", installments = 1 }
        }
    };

    private static JsonElement Field(JsonElement response, string name) => response.GetProperty("data").GetProperty(name);

    private static (string Code, string Message)[] Errors(JsonElement payload)
        => payload.GetProperty("errors").EnumerateArray()
            .Select(e => (e.GetProperty("code").GetString()!, e.GetProperty("message").GetString()!)).ToArray();

    [Test]
    public async Task Sem_login_e_nao_autenticado()
    {
        var response = await Execute("{ myOrders { hasMore } }", new ClaimsPrincipal(new ClaimsIdentity()));

        response.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString()
            .Should().Be("AUTH_NOT_AUTHENTICATED");
        _sent.Should().BeNull();
    }

    [Test]
    public async Task Sem_email_confirmado_e_nao_autorizado()
    {
        var response = await Execute(PlaceOrder, Customer(emailVerified: "false"), OrderInput);

        response.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString()
            .Should().Be("AUTH_NOT_AUTHORIZED");
        _sent.Should().BeNull();
    }

    [Test]
    public async Task Pedido_registrado_com_o_cliente_e_o_email_do_token()
    {
        Returns(new CreateOrderResult(CreateOrderOutcome.Created, "A123456789012", "Pending", 20m, []));

        var payload = Field(await Execute(PlaceOrder, Customer(), OrderInput), "placeOrder");

        payload.GetProperty("order").GetProperty("orderNumber").GetString().Should().Be("A123456789012");
        payload.GetProperty("order").GetProperty("maxAttempts").GetInt32().Should().Be(Order.MaxPaymentAttempts);
        Errors(payload).Should().BeEmpty();
        var command = _sent.Should().BeOfType<CreateCommand>().Subject;
        command.UserId.Should().Be(TokenCustomer);
        command.Email.Should().Be("cliente@exemplo.com");
        command.Items.Single().Should().BeEquivalentTo(new { ProductId = 42, Units = 2, UnitPrice = 0m });
        command.Payment!.CardToken.Should().Be("tok_1");
        command.Customer!.Cpf.Should().Be("52998224725");
    }

    [Test]
    public async Task Pedido_invalido_vira_VALIDATION_com_as_mensagens()
    {
        Returns(CreateOrderResult.Invalid(["CPF inválido.", "UF inválida."]));

        var payload = Field(await Execute(PlaceOrder, Customer(), OrderInput), "placeOrder");

        payload.GetProperty("order").ValueKind.Should().Be(JsonValueKind.Null);
        Errors(payload).Should().Equal(("VALIDATION", "CPF inválido."), ("VALIDATION", "UF inválida."));
    }

    [Test]
    public async Task Catalog_fora_vira_CATALOG_UNAVAILABLE()
    {
        Returns(CreateOrderResult.Unavailable("Não foi possível consultar o catálogo."));

        var payload = Field(await Execute(PlaceOrder, Customer(), OrderInput), "placeOrder");

        Errors(payload).Should().Equal(("CATALOG_UNAVAILABLE", "Não foi possível consultar o catálogo."));
    }

    [Test]
    public async Task Sem_endereco_a_falta_chega_a_validacao()
    {
        Returns(CreateOrderResult.Invalid(["Informe a rua."]));

        await Execute(PlaceOrder, Customer(), new { input = new { items = Array.Empty<object>() } });

        _sent.Should().BeOfType<CreateCommand>().Which.Address.Should().NotBeNull();
    }

    [TestCase(RetryOrderPaymentOutcome.NotFound, "NOT_FOUND")]
    [TestCase(RetryOrderPaymentOutcome.Conflict, "CONFLICT")]
    [TestCase(RetryOrderPaymentOutcome.Invalid, "VALIDATION")]
    public async Task Desfechos_da_nova_tentativa_viram_codigos(RetryOrderPaymentOutcome outcome, string code)
    {
        Returns(new RetryOrderPaymentResult(outcome, null, null, 0, ["motivo"]));

        var payload = Field(await Execute(Retry, Customer(),
            new { input = new { orderNumber = "A999", cardToken = "tok_2" } }), "retryOrderPayment");

        Errors(payload).Should().Equal((code, "motivo"));
        payload.GetProperty("attempt").ValueKind.Should().Be(JsonValueKind.Null);
        _sent.Should().BeOfType<RetryOrderPaymentCommand>().Which.CustomerId.Should().Be(TokenCustomer);
    }

    [Test]
    public async Task Nova_tentativa_aceita_devolve_a_tentativa()
    {
        Returns(new RetryOrderPaymentResult(RetryOrderPaymentOutcome.Accepted, "A123", "Pending", 2, []));

        var payload = Field(await Execute(Retry, Customer(),
            new { input = new { orderNumber = "A123", cardToken = "tok_2" } }), "retryOrderPayment");

        payload.GetProperty("attempt").GetInt32().Should().Be(2);
        Errors(payload).Should().BeEmpty();
    }

    [Test]
    public async Task Pedido_de_outro_cliente_e_nulo_e_a_consulta_usa_o_cliente_do_token()
    {
        Returns<CustomerOrderDetail?>(null);

        var response = await Execute("{ myOrder(orderNumber: \"A999\") { orderNumber } }", Customer());

        Field(response, "myOrder").ValueKind.Should().Be(JsonValueKind.Null);
        response.TryGetProperty("errors", out _).Should().BeFalse();
        _sent.Should().BeOfType<GetCustomerOrderQuery>().Which.CustomerId.Should().Be(TokenCustomer);
    }

    [Test]
    public async Task Meus_pedidos_usam_o_cliente_do_token_e_a_pagina_padrao()
    {
        Returns(new CustomerOrderPage([], 1, 10, false));

        await Execute("{ myOrders { hasMore } }", Customer());

        _sent.Should().BeOfType<GetCustomerOrdersQuery>().Which.Should().BeEquivalentTo(
            new { CustomerId = TokenCustomer, Page = 1, PageSize = CustomerOrdersHandler.DefaultPageSize });
    }
}
