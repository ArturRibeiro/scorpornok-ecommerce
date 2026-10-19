namespace Customers.Tests.CommandHandlers;

[TestFixture]
public class CustomerProfileHandlersTests
{
    private static readonly Guid CustomerId = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");
    private const string Email = "cliente@exemplo.com";

    private Mock<ICustomerRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Customer? _stored;
    private CustomerProfileHandlers _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _stored = null;
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository = new Mock<ICustomerRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);
        _repository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _stored?.Id == id ? _stored : null);
        _repository.Setup(r => r.Add(It.IsAny<Customer>())).Callback((Customer c) => _stored = c);
        _handler = new CustomerProfileHandlers(_repository.Object);
    }

    private static AddressData Address(string city = "Rio de Janeiro")
        => new("Rua das Flores", "100", null, "Centro", city, "RJ", "20040-020");

    private static UpdateCustomerProfileCommand Command(string? cpf = "529.982.247-25", string city = "Rio de Janeiro",
        string email = Email)
        => new(CustomerId, email, "Maria Souza", "(21) 99999-8888", cpf, Address(city));

    [Test]
    public async Task Primeiro_acesso_devolve_so_o_email()
    {
        var profile = await _handler.Handle(new GetCustomerProfileQuery(CustomerId, Email), CancellationToken.None);

        profile.Should().Be(CustomerProfile.Empty(Email));
    }

    [Test]
    public async Task Salvar_cria_o_cadastro_e_ele_e_lido_de_novo()
    {
        var result = await _handler.Handle(Command(), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        _repository.Verify(r => r.Add(It.Is<Customer>(c => c.Id == CustomerId)), Times.Once);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);

        var profile = await _handler.Handle(new GetCustomerProfileQuery(CustomerId, Email), CancellationToken.None);
        profile.FullName.Should().Be("Maria Souza");
        profile.Cpf.Should().Be("52998224725");
        profile.Address!.ZipCode.Should().Be("20040020");
    }

    [Test]
    public async Task Salvar_de_novo_substitui_o_anterior_e_atualiza_o_email()
    {
        await _handler.Handle(Command(), CancellationToken.None);

        var result = await _handler.Handle(Command(city: "Niterói", email: "novo@exemplo.com"), CancellationToken.None);

        result.Profile!.Address!.City.Should().Be("Niterói");
        result.Profile.Email.Should().Be("novo@exemplo.com");
        _repository.Verify(r => r.Add(It.IsAny<Customer>()), Times.Once);
    }

    [Test]
    public async Task Erro_de_validacao_nao_grava_e_indica_o_campo()
    {
        var result = await _handler.Handle(Command(cpf: "111.111.111-11"), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Keys.Should().Equal("cpf");
        _repository.Verify(r => r.Add(It.IsAny<Customer>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Sem_endereco_o_erro_vem_em_address()
    {
        var result = await _handler.Handle(Command() with { Address = null }, CancellationToken.None);

        result.Errors.Keys.Should().Equal("address");
    }
}
