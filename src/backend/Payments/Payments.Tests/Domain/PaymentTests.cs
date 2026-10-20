namespace Payments.Tests.Domain;

[TestFixture]
public class PaymentTests
{
    private static Payment Start(string cardNumber = "4111 1111 1111 1234", int attempt = 1)
        => Payment.Start(7, "A123", attempt, 25m, 2, "Fulano de Tal", cardNumber);

    [Test]
    public void Start_deve_criar_em_processamento_sem_resultado()
    {
        var payment = Start();

        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.Approved.Should().BeFalse();
        payment.Reason.Should().BeNull();
        payment.ProcessedAt.Should().BeNull();
        payment.RequestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        payment.Id.Should().NotBeEmpty();
        payment.OrderNumber.Should().Be("A123");
        payment.Amount.Should().Be(25m);
        payment.Installments.Should().Be(2);
    }

    [Test]
    public void Chave_de_idempotencia_deve_ter_o_numero_do_pedido_e_a_tentativa()
    {
        Start().IdempotencyKey.Should().Be("A123-1");
        Start(attempt: 2).IdempotencyKey.Should().Be("A123-2");
        Payment.KeyFor("A123", 1).Should().Be("A123-1");
    }

    [Test]
    public void Complete_aprovado_deve_aprovar_e_registrar_a_data()
    {
        var payment = Start();

        payment.Complete(new PaymentResult(true)).Should().BeTrue();

        payment.Status.Should().Be(PaymentStatus.Approved);
        payment.Approved.Should().BeTrue();
        payment.Reason.Should().BeNull();
        payment.ProcessedAt.Should().NotBeNull();
    }

    [Test]
    public void Complete_recusado_deve_guardar_o_motivo()
    {
        var payment = Start();

        payment.Complete(new PaymentResult(false, "Cartão recusado")).Should().BeTrue();

        payment.Status.Should().Be(PaymentStatus.Rejected);
        payment.Approved.Should().BeFalse();
        payment.Reason.Should().Be("Cartão recusado");
    }

    [Test]
    public void Complete_repetido_nao_deve_alterar_o_resultado()
    {
        var payment = Start();
        payment.Complete(new PaymentResult(true));
        var processedAt = payment.ProcessedAt;

        payment.Complete(new PaymentResult(false, "outro resultado")).Should().BeFalse();

        payment.Status.Should().Be(PaymentStatus.Approved);
        payment.Reason.Should().BeNull();
        payment.ProcessedAt.Should().Be(processedAt);
    }

    [Test]
    public void Deve_guardar_so_os_4_ultimos_digitos_do_cartao()
    {
        Start().CardLast4.Should().Be("1234");
    }

    [Test]
    public void Nao_deve_ter_campo_para_CVV_nem_guardar_o_numero_completo()
    {
        var payment = Start("4111111111111234");
        payment.Complete(new PaymentResult(true));

        var properties = typeof(Payment).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        properties.Select(p => p.Name).Should().NotContain(name => name.Contains("Cvv", StringComparison.OrdinalIgnoreCase));
        properties.Where(p => p.PropertyType == typeof(string))
            .Select(p => (string?)p.GetValue(payment))
            .Should().NotContain(value => value != null && value.Contains("4111111111111234"));
    }
}
