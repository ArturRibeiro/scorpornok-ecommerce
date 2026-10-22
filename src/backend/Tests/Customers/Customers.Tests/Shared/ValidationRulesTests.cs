namespace Customers.Tests.Shared;

[TestFixture]
public class ValidationRulesTests
{
    [TestCase("529.982.247-25")]
    [TestCase("52998224725")]
    public void Cpf_valido_com_ou_sem_pontuacao(string cpf) => Cpf.IsValid(cpf).Should().BeTrue();

    [TestCase("529.982.247-24", Description = "dígito verificador errado")]
    [TestCase("111.111.111-11", Description = "todos iguais")]
    [TestCase("5299822472", Description = "10 dígitos")]
    [TestCase("529.982.247-2a")]
    [TestCase("")]
    [TestCase(null)]
    public void Cpf_invalido(string? cpf) => Cpf.IsValid(cpf).Should().BeFalse();

    [Test]
    public void Cpf_e_normalizado_para_digitos() => Cpf.Normalize("529.982.247-25").Should().Be("52998224725");

    [Test]
    public void Pontuacao_do_telefone_e_do_cep_e_removida()
    {
        Phone.Normalize("(21) 99999-8888").Should().Be("21999998888");
        ZipCode.Normalize("20040-020").Should().Be("20040020");
    }

    [TestCase("(21) 99999-8888")]
    [TestCase("2133334444")]
    public void Telefone_valido_com_DDD(string phone) => Phone.IsValid(phone).Should().BeTrue();

    [TestCase("99999-8888", Description = "sem DDD")]
    [TestCase("(01) 99999-8888", Description = "DDD com zero")]
    [TestCase("219999988881", Description = "12 dígitos")]
    [TestCase("(21) 9999a-8888")]
    [TestCase(null)]
    public void Telefone_invalido(string? phone) => Phone.IsValid(phone).Should().BeFalse();

    [TestCase("20040-020")]
    [TestCase("20040020")]
    public void Cep_valido(string zipCode) => ZipCode.IsValid(zipCode).Should().BeTrue();

    [TestCase("1234-567")]
    [TestCase("200400200")]
    [TestCase("2004O-020")]
    [TestCase(null)]
    public void Cep_invalido(string? zipCode) => ZipCode.IsValid(zipCode).Should().BeFalse();

    [TestCase("SP")]
    [TestCase("rj")]
    [TestCase("DF")]
    public void Uf_valida(string state) => BrazilianState.IsValid(state).Should().BeTrue();

    [TestCase("XX")]
    [TestCase("São Paulo")]
    [TestCase(null)]
    public void Uf_invalida(string? state) => BrazilianState.IsValid(state).Should().BeFalse();

    [Test]
    public void Nome_com_duas_palavras_e_valido() => PersonName.IsValid("  Maria   Souza ").Should().BeTrue();

    [TestCase("Maria")]
    [TestCase("Maria 123")]
    [TestCase("")]
    [TestCase(null)]
    public void Nome_com_menos_de_duas_palavras_e_invalido(string? name) => PersonName.IsValid(name).Should().BeFalse();

    [Test]
    public void Nome_acima_de_120_caracteres_e_invalido()
        => PersonName.IsValid("Maria " + new string('a', 120)).Should().BeFalse();

    [Test]
    public void Nome_e_normalizado() => PersonName.Normalize("  Maria   Souza ").Should().Be("Maria Souza");
}
