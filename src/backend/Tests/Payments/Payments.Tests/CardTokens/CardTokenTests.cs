namespace Payments.Tests.CardTokens;

[TestFixture]
public class CardTokenTests
{
    private Mock<ICardTokenRepository> _repository = null!;
    private CardToken? _saved;
    private CreateCardTokenHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _saved = null;
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.SaveEntitiesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _repository = new Mock<ICardTokenRepository>();
        _repository.SetupGet(r => r.UnitOfWork).Returns(unitOfWork.Object);
        _repository.Setup(r => r.Add(It.IsAny<CardToken>())).Callback((CardToken t) => _saved = t);
        _handler = new CreateCardTokenHandler(_repository.Object);
    }

    private static CreateCardTokenCommand Card(string number = "4111 1111 1111 1234", string cvv = "123",
        string month = "12", string year = "2030")
        => new("Fulano de Tal", number, month, year, cvv);

    [Test]
    public async Task Cartao_valido_deve_gerar_token_sem_numero_completo_nem_CVV()
    {
        var result = await _handler.Handle(Card(), CancellationToken.None);

        result.Created.Should().BeTrue();
        result.Token.Should().MatchRegex("^tok_[0-9a-f]{32}$");
        result.Last4.Should().Be("1234");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
        _saved!.Id.Should().Be(result.Token);
        _saved.CanBeUsed(DateTime.UtcNow).Should().BeTrue();

        var properties = typeof(CardToken).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        properties.Select(p => p.Name).Should().NotContain(n => n.Contains("Cvv", StringComparison.OrdinalIgnoreCase));
        properties.Where(p => p.PropertyType == typeof(string)).Select(p => (string?)p.GetValue(_saved))
            .Should().NotContain(v => v != null && (v.Contains("411111111111") || v.Contains("4111 1111")));
    }

    [TestCase("12", "cvv")]
    [TestCase("12345", "cvv")]
    public async Task CVV_invalido_deve_recusar_sem_gravar(string cvv, string field)
    {
        var result = await _handler.Handle(Card(cvv: cvv), CancellationToken.None);

        result.Created.Should().BeFalse();
        result.Errors.Should().ContainKey(field);
        _repository.Verify(r => r.Add(It.IsAny<CardToken>()), Times.Never);
    }

    [Test]
    public async Task Numero_curto_e_cartao_vencido_devem_indicar_os_campos()
    {
        var result = await _handler.Handle(Card(number: "4111", year: "2020"), CancellationToken.None);

        result.Errors.Keys.Should().BeEquivalentTo(["cardNumber", "expirationYear"]);
    }

    [Test]
    public void Token_vencido_ou_usado_nao_pode_ser_usado()
    {
        var now = DateTime.UtcNow;
        var token = CardToken.Create("Fulano", "4111111111111111", "12", "2030", now);

        token.CanBeUsed(now.AddHours(25)).Should().BeFalse();
        token.MarkUsed(now);
        token.CanBeUsed(now).Should().BeFalse();
    }

    [Test]
    public async Task Endpoint_deve_responder_201_com_o_token_e_400_com_os_erros()
    {
        var bus = new Mock<IMemoryBus>();
        bus.Setup(b => b.RequestAsync(It.IsAny<Shared.Code.Commands.Message<CreateCardTokenResult>>()))
            .Returns((Shared.Code.Commands.Message<CreateCardTokenResult> c) => _handler.Handle((CreateCardTokenCommand)c, CancellationToken.None));

        var created = await CreateCardTokenAsync(bus.Object,
            new CardTokenRequest("Fulano", "4111111111111111", "12", "2030", "123"));
        var invalid = await CreateCardTokenAsync(bus.Object,
            new CardTokenRequest("Fulano", "4111111111111111", "12", "2030", "1"));

        created.Should().BeAssignableTo<IStatusCodeHttpResult>().Which.StatusCode.Should().Be(StatusCodes.Status201Created);
        invalid.Should().BeAssignableTo<IStatusCodeHttpResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
}
