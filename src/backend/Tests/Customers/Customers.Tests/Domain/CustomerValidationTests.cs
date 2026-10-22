namespace Customers.Tests.Domain;

[TestFixture]
public class CustomerValidationTests
{
    private static readonly Guid CustomerId = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private static Address ValidAddress(string? complement = "Apto 12", string state = "RJ", string zipCode = "20040-020")
        => Address.Create("Rua das Flores", "100", complement, "Centro", "Rio de Janeiro", state, zipCode);

    private static Customer Customer(string? fullName = "Maria Souza", string? phone = "(21) 99999-8888",
        string? cpf = "529.982.247-25", Address? address = null, bool withAddress = true)
    {
        var customer = new Customer(CustomerId, "cliente@exemplo.com");
        customer.Update("cliente@exemplo.com", fullName, phone, cpf, withAddress ? address ?? ValidAddress() : null);
        return customer;
    }

    private static IEnumerable<string> FailedFields(Customer customer)
        => customer.Validate().Errors.Select(e => e.PropertyName);

    [Test]
    public void Cadastro_completo_e_valido()
    {
        var customer = Customer();

        customer.IsValid().Should().BeTrue();
        customer.Errors.Should().BeEmpty();
    }

    [Test]
    public void Pontuacao_e_aceita_e_removida()
    {
        var customer = Customer();

        customer.IsValid().Should().BeTrue();
        customer.Cpf.Should().Be("52998224725");
        customer.Phone.Should().Be("21999998888");
        customer.Address!.ZipCode.Should().Be("20040020");
    }

    [Test]
    public void Complemento_vazio_e_aceito()
    {
        var customer = Customer(address: ValidAddress(complement: "  "));

        customer.IsValid().Should().BeTrue();
        customer.Address!.Complement.Should().BeNull();
    }

    [TestCase("111.111.111-11")]
    [TestCase("529.982.247-24")]
    public void Cpf_invalido_e_indicado_no_campo_cpf(string cpf)
        => FailedFields(Customer(cpf: cpf)).Should().Equal("cpf");

    [Test]
    public void Uf_invalida_e_indicada_no_campo_do_endereco()
        => FailedFields(Customer(address: ValidAddress(state: "XX"))).Should().Equal("address.state");

    [Test]
    public void Cep_invalido_e_indicado_no_campo_do_endereco()
        => FailedFields(Customer(address: ValidAddress(zipCode: "1234-567"))).Should().Equal("address.zipCode");

    [Test]
    public void Campos_obrigatorios_faltando_sao_indicados_um_a_um()
    {
        var customer = Customer(phone: null, cpf: "",
            address: Address.Create("Rua das Flores", null, null, null, null, null, "20040-020"));

        customer.IsValid().Should().BeFalse();
        FailedFields(customer).Should().BeEquivalentTo(
            "phone", "cpf", "address.number", "address.district", "address.city", "address.state");
    }

    [Test]
    public void Sem_endereco_o_erro_vem_em_address()
        => FailedFields(Customer(withAddress: false)).Should().Equal("address");

    [Test]
    public void Nome_com_uma_palavra_e_invalido()
        => FailedFields(Customer(fullName: "Maria")).Should().Equal("fullName");

    [Test]
    public void Atualizar_substitui_o_cadastro_anterior()
    {
        var customer = Customer();

        customer.Update("novo@exemplo.com", "João Lima", "2133334444", "52998224725",
            Address.Create("Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "sp", "01310100"));

        customer.Email.Should().Be("novo@exemplo.com");
        customer.FullName.Should().Be("João Lima");
        customer.Address!.State.Should().Be("SP");
        customer.Address.Complement.Should().BeNull();
    }
}
