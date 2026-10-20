namespace Customers.Tests.Web;

// O cadastro no GraphQL (subgraph do gateway): mesma regra de acesso e mesmos erros por campo do REST.
[TestFixture]
public class CustomerProfileGraphTests
{
    private static readonly Guid CustomerId = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private Mock<IMemoryBus> _bus = null!;
    private object? _sent;

    [SetUp]
    public void SetUp()
    {
        _sent = null;
        _bus = new Mock<IMemoryBus>();
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CustomerProfile>>()))
            .Callback((Message<CustomerProfile> q) => _sent = q)
            .ReturnsAsync(new CustomerProfile("cliente@exemplo.com", "Maria Souza", "11999998888", "52998224725", null));
    }

    private static ClaimsPrincipal Customer(string emailVerified = "true")
        => new(new ClaimsIdentity([new Claim("sub", CustomerId.ToString()), new Claim("email", "cliente@exemplo.com"),
            new Claim("email_verified", emailVerified)], "Bearer"));

    private async Task<JsonElement> Execute(string query, ClaimsPrincipal? user = null, object? variables = null)
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

        var request = OperationRequestBuilder.New().SetDocument(query);
        if (user is not null) request.SetUser(user);
        if (variables is not null)
            request.SetVariableValues(JsonSerializer.Serialize(variables));
        var result = await executor.ExecuteAsync(request.Build());
        return JsonDocument.Parse(result.ToJson()).RootElement;
    }

    private static string ErrorCode(JsonElement response)
        => response.GetProperty("errors")[0].GetProperty("extensions").GetProperty("code").GetString()!;

    [Test]
    public async Task Sem_login_e_nao_autenticado()
    {
        // Como no ASP.NET sem token: um usuário anônimo, sem identidade autenticada.
        var response = await Execute("{ me { email } }", new ClaimsPrincipal(new ClaimsIdentity()));

        ErrorCode(response).Should().Be("AUTH_NOT_AUTHENTICATED");
        _sent.Should().BeNull();
    }

    [Test]
    public async Task Sem_email_confirmado_e_nao_autorizado()
    {
        var response = await Execute("{ me { email } }", Customer(emailVerified: "false"));

        ErrorCode(response).Should().Be("AUTH_NOT_AUTHORIZED");
        _sent.Should().BeNull();
    }

    [Test]
    public async Task Le_o_cadastro_do_cliente_do_token()
    {
        var response = await Execute("{ me { email fullName cpf } }", Customer());

        response.GetProperty("data").GetProperty("me").GetProperty("fullName").GetString().Should().Be("Maria Souza");
        _sent.Should().BeOfType<GetCustomerProfileQuery>().Which.CustomerId.Should().Be(CustomerId);
    }

    [Test]
    public async Task Erros_de_validacao_voltam_por_campo_sem_cadastro()
    {
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<UpdateCustomerProfileResult>>()))
            .Callback((Message<UpdateCustomerProfileResult> c) => _sent = c)
            .ReturnsAsync(UpdateCustomerProfileResult.Invalid(new Dictionary<string, string[]>
            {
                ["cpf"] = ["CPF inválido."], ["address.zipCode"] = ["CEP inválido."]
            }));

        var response = await Execute(
            "mutation($input: UpdateMyProfileInput!) { updateMyProfile(input: $input) { profile { email } errors { field messages } } }",
            Customer(), new { input = new { cpf = "111.111.111-11", address = new { zipCode = "1" } } });

        var payload = response.GetProperty("data").GetProperty("updateMyProfile");
        payload.GetProperty("profile").ValueKind.Should().Be(JsonValueKind.Null);
        payload.GetProperty("errors").EnumerateArray()
            .Select(e => (e.GetProperty("field").GetString(), e.GetProperty("messages")[0].GetString()))
            .Should().BeEquivalentTo([("cpf", "CPF inválido."), ("address.zipCode", "CEP inválido.")]);
        var command = _sent.Should().BeOfType<UpdateCustomerProfileCommand>().Subject;
        command.CustomerId.Should().Be(CustomerId);
        command.Email.Should().Be("cliente@exemplo.com");
        command.Cpf.Should().Be("111.111.111-11");
        command.Address!.ZipCode.Should().Be("1");
    }
}
