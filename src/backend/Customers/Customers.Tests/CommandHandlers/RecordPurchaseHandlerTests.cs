namespace Customers.Tests.CommandHandlers;

[TestFixture]
public class RecordPurchaseHandlerTests
{
    private static readonly Guid CustomerId = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private Mock<ICustomerRepository> _repository = null!;
    private Mock<IUnitOfWork> _unitOfWork = null!;
    private Customer? _stored;
    private List<DeliveryAddress> _history = null!;
    private RecordPurchaseHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _stored = null;
        _history = [];
        _unitOfWork = new Mock<IUnitOfWork>();
        _unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository = new Mock<ICustomerRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(_unitOfWork.Object);
        _repository.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => _stored?.Id == id ? _stored : null);
        _repository.Setup(r => r.Add(It.IsAny<Customer>())).Callback((Customer c) => _stored = c);
        _repository.Setup(r => r.HasDeliveryAddressAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string number, CancellationToken _) => _history.Any(h => h.OrderNumber == number));
        _repository.Setup(r => r.AddDeliveryAddress(It.IsAny<DeliveryAddress>()))
            .Callback((DeliveryAddress d) => _history.Add(d));
        _handler = new RecordPurchaseHandler(_repository.Object, NullLogger<RecordPurchaseHandler>.Instance);
    }

    private static RecordPurchaseCommand Purchase(string orderNumber = "A123", string city = "Rio de Janeiro",
        string phone = "21999998888", string cpf = "52998224725")
        => new(orderNumber, CustomerId, "cliente@exemplo.com", "Maria Souza", phone, cpf,
            new AddressData("Rua das Flores", "100", null, "Centro", city, "RJ", "20040020"),
            new DateTime(2026, 10, 18, 10, 0, 0));

    private static Customer Existing()
    {
        var customer = new Customer(CustomerId, "cliente@exemplo.com");
        customer.Update("cliente@exemplo.com", "Maria Souza", "1133334444", "52998224725",
            Address.Create("Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310100"));
        return customer;
    }

    [Test]
    public async Task Primeira_compra_cria_o_cadastro_com_o_comprador_e_o_endereco_da_entrega()
    {
        await _handler.Handle(Purchase(), CancellationToken.None);

        _stored.Should().NotBeNull();
        _stored!.FullName.Should().Be("Maria Souza");
        _stored.Phone.Should().Be("21999998888");
        _stored.Cpf.Should().Be("52998224725");
        _stored.Address!.City.Should().Be("Rio de Janeiro");
        _history.Should().ContainSingle(h => h.OrderNumber == "A123" && h.CustomerId == CustomerId);
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Compra_leva_o_numero_do_pedido_ao_span_corrente()
    {
        using var span = new Activity("order-placed process").Start();

        await _handler.Handle(Purchase(), CancellationToken.None);

        span.GetTagItem(OrderTelemetry.OrderNumberTag).Should().Be("A123");
    }

    [Test]
    public async Task Cadastro_existente_nao_muda_e_o_endereco_entra_no_historico()
    {
        _stored = Existing();

        await _handler.Handle(Purchase(phone: "21988887777"), CancellationToken.None);

        _stored.Address!.City.Should().Be("São Paulo");
        _stored.Phone.Should().Be("1133334444");
        _repository.Verify(r => r.Add(It.IsAny<Customer>()), Times.Never);
        _history.Should().ContainSingle(h => h.Address.City == "Rio de Janeiro");
    }

    [Test]
    public async Task Pedido_recebido_de_novo_nao_gera_outro_registro()
    {
        await _handler.Handle(Purchase(), CancellationToken.None);
        await _handler.Handle(Purchase(), CancellationToken.None);

        _history.Should().ContainSingle();
        _unitOfWork.Verify(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Dados_invalidos_guardam_o_historico_sem_criar_o_cadastro()
    {
        await _handler.Handle(Purchase(cpf: "11111111111"), CancellationToken.None);

        _stored.Should().BeNull();
        _history.Should().ContainSingle();
    }
}
