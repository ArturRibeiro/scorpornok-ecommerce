using Gateway.Payment.Data.Gateways;

namespace Gateway.Payment.Tests.Data;

[TestFixture]
public class PaymentGatewayTests
{
    private static PaymentRequest Request(string cardNumber)
        => new(25m, "Fulano de Tal", cardNumber, "12", "2030", "123", 1);

    [TestCase("4111111111111111")]
    [TestCase("4111 1111 1111 1234")]
    public async Task Deve_aprovar_cartao_que_nao_termina_em_0000(string cardNumber)
    {
        var result = await new PaymentGateway().SendProcessPaymentAsync(Request(cardNumber));

        result.Approved.Should().BeTrue();
        result.Reason.Should().BeNull();
    }

    [TestCase("4111111111110000")]
    [TestCase("4111 1111 1111 0000")]
    public async Task Deve_recusar_cartao_que_termina_em_0000(string cardNumber)
    {
        var result = await new PaymentGateway().SendProcessPaymentAsync(Request(cardNumber));

        result.Approved.Should().BeFalse();
        result.Reason.Should().NotBeNullOrEmpty();
    }
}
