namespace Orders.Tests.Web;

[TestFixture]
public class CustomerOrdersEndpointTests
{
    private static readonly Guid TokenCustomer = Guid.Parse("6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11");

    private Mock<IMemoryBus> _bus = null!;
    private object? _sent;

    [SetUp]
    public void SetUp()
    {
        _sent = null;
        _bus = new Mock<IMemoryBus>();
        _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CustomerOrderPage>>()))
            .Callback((Message<CustomerOrderPage> query) => _sent = query)
            .ReturnsAsync(new CustomerOrderPage([], 1, 10, false));
    }

    private void DetailReturns(CustomerOrderDetail? detail)
        => _bus.Setup(b => b.RequestAsync(It.IsAny<Message<CustomerOrderDetail?>>()))
            .Callback((Message<CustomerOrderDetail?> query) => _sent = query)
            .ReturnsAsync(detail);

    private static ClaimsPrincipal Customer()
        => new(new ClaimsIdentity([new Claim("sub", TokenCustomer.ToString()), new Claim("email", "cliente@exemplo.com"),
            new Claim("email_verified", "true")], "Bearer"));

    private static ClaimsPrincipal WithoutIdentity()
        => new(new ClaimsIdentity([new Claim("email", "cliente@exemplo.com")], "Bearer"));

    private static int? StatusOf(IResult result) => ((IStatusCodeHttpResult)result).StatusCode;

    [Test]
    public async Task Lista_usa_o_cliente_do_token_e_a_pagina_padrao()
    {
        var result = await GetMyOrdersAsync(_bus.Object, Customer(), null, null);

        StatusOf(result).Should().Be(StatusCodes.Status200OK);
        _sent.Should().BeOfType<GetCustomerOrdersQuery>().Which.Should().BeEquivalentTo(
            new { CustomerId = TokenCustomer, Page = 1, PageSize = CustomerOrdersHandler.DefaultPageSize });
    }

    [Test]
    public async Task Lista_repassa_a_pagina_pedida()
    {
        await GetMyOrdersAsync(_bus.Object, Customer(), 3, 20);

        _sent.Should().BeOfType<GetCustomerOrdersQuery>().Which.Should().BeEquivalentTo(
            new { CustomerId = TokenCustomer, Page = 3, PageSize = 20 });
    }

    [Test]
    public async Task Detalhe_do_dono_responde_200()
    {
        DetailReturns(new CustomerOrderDetail("A123456789012", DateTime.Now, "Confirmed", 10m, 1, 3, false,
            new CustomerOrderAddress("Rua A", "10", null, "Centro", "São Paulo", "SP", "01001000"), []));

        var result = await GetMyOrderAsync(_bus.Object, "A123456789012", Customer());

        StatusOf(result).Should().Be(StatusCodes.Status200OK);
        _sent.Should().BeOfType<GetCustomerOrderQuery>().Which.Should().BeEquivalentTo(
            new { CustomerId = TokenCustomer, OrderNumber = "A123456789012" });
    }

    [Test]
    public async Task Pedido_nao_encontrado_responde_404_com_a_mensagem()
    {
        DetailReturns(null);

        var result = await GetMyOrderAsync(_bus.Object, "A000000000000", Customer());

        StatusOf(result).Should().Be(StatusCodes.Status404NotFound);
        ((IValueHttpResult)result).Value.Should().BeEquivalentTo(new { Errors = new[] { "Pedido não encontrado." } });
    }

    [Test]
    public async Task Token_sem_identidade_completa_nao_chega_ao_bus()
    {
        (await GetMyOrdersAsync(_bus.Object, WithoutIdentity(), null, null)).Should().BeOfType<ForbidHttpResult>();
        (await GetMyOrderAsync(_bus.Object, "A123456789012", WithoutIdentity())).Should().BeOfType<ForbidHttpResult>();
        _sent.Should().BeNull();
    }

    [Test]
    public async Task Os_dois_endpoints_exigem_cliente_com_email_confirmado()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton(_bus.Object);
        await using var app = builder.Build();
        app.GetMyOrders();
        app.GetMyOrder();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(d => d.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains("GET") == true)
            .ToList();

        endpoints.Select(e => e.RoutePattern.RawText).Should().BeEquivalentTo("/orders", "/orders/{orderNumber}");
        endpoints.Should().AllSatisfy(e => e.Metadata.GetOrderedMetadata<IAuthorizeData>()
            .Should().Contain(a => a.Policy == CustomerAuthentication.VerifiedCustomerPolicy));
    }
}
