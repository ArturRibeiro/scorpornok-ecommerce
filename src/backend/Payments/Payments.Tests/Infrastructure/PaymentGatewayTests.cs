using Payments.Infrastructure.Gateways;

namespace Payments.Tests.Infrastructure;

[TestFixture]
public class PaymentGatewayTests
{
    // O gateway recebe o cartão tokenizado: titular e 4 últimos dígitos.
    private static PaymentRequest Request(string cardNumber, string key = "A123-1")
        => new(key, 25m, "Fulano de Tal", cardNumber[^4..], 1);

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

    [Test]
    public async Task Mesma_chave_deve_devolver_o_resultado_original_sem_reavaliar()
    {
        var gateway = new PaymentGateway();
        var first = await gateway.SendProcessPaymentAsync(Request("4111111111111111"));

        // Mesma chave com outro cartão: um gateway real não cobraria de novo nem mudaria a resposta.
        var repeated = await gateway.SendProcessPaymentAsync(Request("4111111111110000"));

        repeated.Should().Be(first);
        repeated.Approved.Should().BeTrue();
    }

    [Test]
    public async Task Chaves_diferentes_devem_ser_cobrancas_independentes()
    {
        var gateway = new PaymentGateway();

        var approved = await gateway.SendProcessPaymentAsync(Request("4111111111111111", key: "A123-1"));
        var rejected = await gateway.SendProcessPaymentAsync(Request("4111111111110000", key: "A123-2"));

        approved.Approved.Should().BeTrue();
        rejected.Approved.Should().BeFalse();
    }
}
