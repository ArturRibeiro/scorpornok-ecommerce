using System.Reflection;

namespace Gateway.Payment.Tests.Domain;

[TestFixture]
public class PaymentTests
{
    private static Payment.Domain.Payment Create(string cardNumber = "4111 1111 1111 1234", PaymentResult? result = null)
        => Payment.Domain.Payment.Create(7, "A123", 25m, 2, "Fulano de Tal", cardNumber, result ?? new PaymentResult(true));

    [Test]
    public void Deve_guardar_so_os_4_ultimos_digitos_do_cartao()
    {
        var payment = Create();

        payment.CardLast4.Should().Be("1234");
        payment.Id.Should().NotBeEmpty();
        payment.OrderNumber.Should().Be("A123");
        payment.Amount.Should().Be(25m);
        payment.Installments.Should().Be(2);
        payment.Approved.Should().BeTrue();
        payment.Reason.Should().BeNull();
    }

    [Test]
    public void Nao_deve_ter_campo_para_CVV_nem_guardar_o_numero_completo()
    {
        var payment = Create("4111111111111234");

        var properties = typeof(Payment.Domain.Payment).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        properties.Select(p => p.Name).Should().NotContain(name => name.Contains("Cvv", StringComparison.OrdinalIgnoreCase));
        properties.Where(p => p.PropertyType == typeof(string))
            .Select(p => (string?)p.GetValue(payment))
            .Should().NotContain(value => value != null && value.Contains("4111111111111234"));
    }

    [Test]
    public void Pagamento_recusado_deve_guardar_o_motivo()
    {
        var payment = Create(result: new PaymentResult(false, "Cartão recusado"));

        payment.Approved.Should().BeFalse();
        payment.Reason.Should().Be("Cartão recusado");
    }
}
