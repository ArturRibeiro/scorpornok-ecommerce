using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;
using Shared.Code;
using static Orders.Web.Api.WebApplicationExtensions.WebApplicationExtensions;

namespace Orders.Tests.Web;

[TestFixture]
public class RetryOrderPaymentEndpointTests
{
    private static readonly Guid TokenCustomer = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private Mock<IMemoryBus> _bus = null!;
    private RetryOrderPaymentCommand? _sent;

    [SetUp]
    public void SetUp()
    {
        _sent = null;
        _bus = new Mock<IMemoryBus>();
    }

    private void Returns(RetryOrderPaymentResult result)
        => _bus.Setup(b => b.RequestAsync(It.IsAny<Message<RetryOrderPaymentResult>>()))
            .Callback((Message<RetryOrderPaymentResult> command) => _sent = (RetryOrderPaymentCommand)command)
            .ReturnsAsync(result);

    private static ClaimsPrincipal Customer()
        => new(new ClaimsIdentity([new Claim("sub", TokenCustomer.ToString()), new Claim("email", "cliente@exemplo.com"),
            new Claim("email_verified", "true")], "Bearer"));

    private static int? StatusOf(IResult result) => ((IStatusCodeHttpResult)result).StatusCode;

    [Test]
    public async Task Aceita_responde_202_com_a_tentativa_e_usa_o_cliente_do_token()
    {
        Returns(new RetryOrderPaymentResult(RetryOrderPaymentOutcome.Accepted, "A123456789012", "Pending", 2, []));

        var result = await RetryOrderPaymentAsync(_bus.Object, "A123456789012", new PaymentMethod("tok_novo"), Customer());

        StatusOf(result).Should().Be(StatusCodes.Status202Accepted);
        _sent!.CustomerId.Should().Be(TokenCustomer);
        _sent.OrderNumber.Should().Be("A123456789012");
        _sent.Payment!.CardToken.Should().Be("tok_novo");
    }

    [TestCase(RetryOrderPaymentOutcome.NotFound, StatusCodes.Status404NotFound)]
    [TestCase(RetryOrderPaymentOutcome.Conflict, StatusCodes.Status409Conflict)]
    [TestCase(RetryOrderPaymentOutcome.Invalid, StatusCodes.Status400BadRequest)]
    public async Task Desfechos_viram_o_status_HTTP(RetryOrderPaymentOutcome outcome, int status)
    {
        Returns(new RetryOrderPaymentResult(outcome, null, null, 0, ["motivo"]));

        var result = await RetryOrderPaymentAsync(_bus.Object, "A123456789012", new PaymentMethod("tok_novo"), Customer());

        StatusOf(result).Should().Be(status);
    }

    [Test]
    public async Task Token_sem_identidade_completa_nao_chega_ao_bus()
    {
        var result = await RetryOrderPaymentAsync(_bus.Object, "A123456789012", new PaymentMethod("tok_novo"),
            new ClaimsPrincipal(new ClaimsIdentity([new Claim("email", "cliente@exemplo.com")], "Bearer")));

        result.Should().BeOfType<ForbidHttpResult>();
        _sent.Should().BeNull();
    }
}
