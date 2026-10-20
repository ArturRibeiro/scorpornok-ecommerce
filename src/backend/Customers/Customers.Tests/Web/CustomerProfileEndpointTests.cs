namespace Customers.Tests.Web;

[TestFixture]
public class CustomerProfileEndpointTests
{
    private static readonly Guid CustomerA = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");
    private static readonly Guid CustomerB = Guid.Parse("0d4e8a1b-77aa-4c2e-8f3d-5a6b7c8d9e00");

    private Mock<IMemoryBus> _bus = null!;
    private GetCustomerProfileQuery? _query;
    private UpdateCustomerProfileCommand? _command;

    [SetUp]
    public void SetUp()
    {
        _query = null;
        _command = null;
        _bus = new Mock<IMemoryBus>();
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CustomerProfile>>()))
            .Callback((Message<CustomerProfile> q) => _query = (GetCustomerProfileQuery)q)
            .ReturnsAsync((Message<CustomerProfile> q) => CustomerProfile.Empty(((GetCustomerProfileQuery)q).Email));
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<UpdateCustomerProfileResult>>()))
            .Callback((Message<UpdateCustomerProfileResult> c) => _command = (UpdateCustomerProfileCommand)c)
            .ReturnsAsync(new UpdateCustomerProfileResult(CustomerProfile.Empty("b@exemplo.com"),
                new Dictionary<string, string[]>()));
    }

    private static ClaimsPrincipal Token(Guid customerId, string email)
        => new(new ClaimsIdentity(
            [new Claim("sub", customerId.ToString()), new Claim("email", email), new Claim("email_verified", "true")],
            "Bearer"));

    private static CustomerProfileRequest Request()
        => new("Maria Souza", "(21) 99999-8888", "529.982.247-25",
            new AddressData("Rua das Flores", "100", null, "Centro", "Rio de Janeiro", "RJ", "20040-020"));

    [Test]
    public async Task Consulta_le_o_cadastro_do_cliente_do_token()
    {
        var result = await GetProfileAsync(_bus.Object, Token(CustomerB, "b@exemplo.com"));

        result.Should().BeOfType<Ok<CustomerProfile>>().Which.Value!.Email.Should().Be("b@exemplo.com");
        _query!.CustomerId.Should().Be(CustomerB);
        _query.Email.Should().Be("b@exemplo.com");
    }

    [Test]
    public async Task Salvar_altera_so_o_cadastro_do_cliente_do_token()
    {
        await UpdateProfileAsync(_bus.Object, Request(), Token(CustomerB, "b@exemplo.com"));

        _command!.CustomerId.Should().Be(CustomerB).And.NotBe(CustomerA);
        _command.Email.Should().Be("b@exemplo.com");
    }

    [Test]
    public async Task Erro_de_validacao_responde_400_com_os_campos()
    {
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<UpdateCustomerProfileResult>>()))
            .ReturnsAsync(UpdateCustomerProfileResult.Invalid(new Dictionary<string, string[]> { ["cpf"] = ["CPF inválido."] }));

        var result = await UpdateProfileAsync(_bus.Object, Request(), Token(CustomerA, "a@exemplo.com"));

        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task Token_sem_identidade_completa_nao_chega_ao_bus()
    {
        var token = new ClaimsPrincipal(new ClaimsIdentity([new Claim("email", "a@exemplo.com")], "Bearer"));

        (await GetProfileAsync(_bus.Object, token)).Should().BeOfType<ForbidHttpResult>();
        (await UpdateProfileAsync(_bus.Object, Request(), token)).Should().BeOfType<ForbidHttpResult>();
        _query.Should().BeNull();
        _command.Should().BeNull();
    }
}
