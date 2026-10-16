using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Code;
using static Orders.Web.Api.WebApplicationExtensions.WebApplicationExtensions;

namespace Orders.Tests.Web;

[TestFixture]
public class CreateOrderEndpointTests
{
    private static readonly Guid TokenCustomer = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");
    private static readonly Guid BodyCustomer = Guid.Parse("0d4e8a1b-77aa-4c2e-8f3d-5a6b7c8d9e00");

    private Mock<IMemoryBus> _bus = null!;
    private CreateCommand? _sent;

    [SetUp]
    public void SetUp()
    {
        _sent = null;
        _bus = new Mock<IMemoryBus>();
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CreateOrderResult>>()))
            .Callback((Message<CreateOrderResult> command) => _sent = (CreateCommand)command)
            .ReturnsAsync(new CreateOrderResult(CreateOrderOutcome.Created, "A123", "Pending", 25m, []));
    }

    private static CreateCommand Body()
        => new(BodyCustomer, new OrderAddressMessageResponse(), [], null!, "outro@exemplo.com");

    private static ClaimsPrincipal Token(params (string Type, string Value)[] claims)
        => new(new ClaimsIdentity(claims.Select(c => new Claim(c.Type, c.Value)), "Bearer"));

    [Test]
    public async Task Cliente_e_email_vem_do_token_e_nao_do_corpo()
    {
        var result = await CreateOrderAsync(_bus.Object, Body(),
            Token(("sub", TokenCustomer.ToString()), ("email", "cliente@exemplo.com"), ("email_verified", "true")));

        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status201Created);
        _sent!.UserId.Should().Be(TokenCustomer);
        _sent.Email.Should().Be("cliente@exemplo.com");
    }

    [Test]
    public async Task Token_sem_identidade_completa_nao_chega_ao_bus()
    {
        var result = await CreateOrderAsync(_bus.Object, Body(), Token(("email", "cliente@exemplo.com")));

        result.Should().BeOfType<ForbidHttpResult>();
        _sent.Should().BeNull();
    }

    [Test]
    public async Task Catalog_indisponivel_deve_responder_503()
    {
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CreateOrderResult>>()))
            .ReturnsAsync(CreateOrderResult.Unavailable("O catálogo está indisponível."));

        var result = await CreateOrderAsync(_bus.Object, Body(),
            Token(("sub", TokenCustomer.ToString()), ("email", "cliente@exemplo.com"), ("email_verified", "true")));

        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Test]
    public async Task Pagamento_sem_token_deve_responder_400()
    {
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CreateOrderResult>>()))
            .ReturnsAsync(CreateOrderResult.Invalid(["Informe o cartão."]));

        var result = await CreateOrderAsync(_bus.Object, Body() with { Payment = new PaymentMethod(null) },
            Token(("sub", TokenCustomer.ToString()), ("email", "cliente@exemplo.com"), ("email_verified", "true")));

        result.Should().BeAssignableTo<IStatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
