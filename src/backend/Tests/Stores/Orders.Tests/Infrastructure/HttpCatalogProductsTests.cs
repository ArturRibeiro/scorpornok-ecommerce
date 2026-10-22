namespace Orders.Tests.Infrastructure;

[TestFixture]
public class HttpCatalogProductsTests
{
    // Responde a toda requisição com a função recebida e guarda a última URL chamada.
    private sealed class FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public Uri? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request.RequestUri;
            return Task.FromResult(respond(request));
        }
    }

    private static HttpCatalogProducts Client(FakeHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("http://catalog/") });

    private static HttpResponseMessage Json(string json)
        => new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Test]
    public async Task Deve_devolver_os_produtos_por_id_com_preco_nome_e_foto()
    {
        var handler = new FakeHandler(_ => Json(
            """[{"id":42,"name":"Camiseta","sku":"S","pictureUri":"/catalog-imgs/S.jpg","description":"d","price":10.00}]"""));

        var products = await Client(handler).GetAsync([42, 42]);

        products.Should().ContainSingle();
        products[42].Should().Be(new CatalogProduct(42, "Camiseta", "/catalog-imgs/S.jpg", 10.00m));
        handler.LastRequest!.ToString().Should().Be("http://catalog/GetProductsByIds?ids=42");
    }

    [Test]
    public async Task Produto_ausente_deve_ficar_fora_do_resultado()
    {
        var handler = new FakeHandler(_ => Json(
            """[{"id":1,"name":"Um","pictureUri":"/a.jpg","price":5}]"""));

        var products = await Client(handler).GetAsync([1, 999]);

        products.Keys.Should().BeEquivalentTo([1]);
        handler.LastRequest!.Query.Should().Be("?ids=1&ids=999");
    }

    [Test]
    public async Task Sem_itens_nao_deve_chamar_o_Catalog()
    {
        var handler = new FakeHandler(_ => throw new InvalidOperationException("não deveria chamar"));

        var products = await Client(handler).GetAsync([]);

        products.Should().BeEmpty();
        handler.LastRequest.Should().BeNull();
    }

    [Test]
    public async Task Erro_do_Catalog_deve_lancar_CatalogUnavailableException()
    {
        var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var act = () => Client(handler).GetAsync([1]);

        await act.Should().ThrowAsync<CatalogUnavailableException>();
    }

    [Test]
    public async Task Falha_de_rede_deve_lancar_CatalogUnavailableException()
    {
        var handler = new FakeHandler(_ => throw new HttpRequestException("connection refused"));

        var act = () => Client(handler).GetAsync([1]);

        await act.Should().ThrowAsync<CatalogUnavailableException>()
            .Where(ex => ex.InnerException is HttpRequestException);
    }

    [Test]
    public async Task Tempo_esgotado_deve_lancar_CatalogUnavailableException()
    {
        var handler = new FakeHandler(_ => throw new TaskCanceledException("timeout"));

        var act = () => Client(handler).GetAsync([1]);

        await act.Should().ThrowAsync<CatalogUnavailableException>();
    }
}
