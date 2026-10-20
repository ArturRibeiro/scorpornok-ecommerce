namespace Ecommerce.Scenarios.Integration.Spec.Tests.Steps;

// O Catalog como subgraph do gateway: o /graphql segue as mesmas regras do REST (spec product-catalog).
[Binding]
public class CatalogGraphQLStepDefinitions : StepBase
{
    private const string PageQuery =
        "query($page: Int, $pageSize: Int) { products(page: $page, pageSize: $pageSize) { " +
        "pageInfo { currentPage pageSize totalItems totalPages hasPreviousPage hasNextPage } " +
        "items { id name sku pictureUri price description } } }";

    private JObject _response = null!;

    private JToken Page => _response["data"]!["products"]!;

    [When(@"consulto pelo GraphQL a página (\d+) com (\d+) produtos")]
    public async Task WhenConsultoPeloGraphQLAPagina(int page, int pageSize)
        => _response = await GraphQLAsync(PageQuery, new { page, pageSize });

    [Then(@"recebo (\d+) produtos e o total do catálogo")]
    public async Task ThenReceboProdutosEOTotal(int count)
    {
        var rest = await SendAsync<PagedList<ProductItemMessageResponse>>("Products?PageNumber=1&PageSize=1");
        Page["items"]!.Should().HaveCount(count);
        Page["pageInfo"]!["totalItems"]!.Value<int>().Should().Be(rest.Value.PageInfo.TotalItems);
    }

    [Then(@"há página seguinte e não há anterior")]
    public void ThenHaPaginaSeguinteENaoHaAnterior()
    {
        Page["pageInfo"]!["hasNextPage"]!.Value<bool>().Should().BeTrue();
        Page["pageInfo"]!["hasPreviousPage"]!.Value<bool>().Should().BeFalse();
    }

    [When(@"consulto pelo GraphQL o produto (\d+)")]
    public async Task WhenConsultoPeloGraphQLOProduto(long id)
        => _response = await GraphQLAsync("query($id: Long!) { productById(id: $id) { id } }", new { id });

    [Then(@"a resposta não tem produto nem erro")]
    public void ThenARespostaNaoTemProdutoNemErro()
    {
        _response["errors"].Should().BeNull();
        _response["data"]!["productById"]!.Type.Should().Be(JTokenType.Null);
    }

    private List<(long Id, decimal Price)> _graphql = [];
    private List<(long Id, decimal Price)> _rest = [];

    [When(@"consulto a página (\d+) com (\d+) produtos pelo GraphQL e pelo REST")]
    public async Task WhenConsultoAPaginaPeloGraphQLEPeloRest(int page, int pageSize)
    {
        _response = await GraphQLAsync(PageQuery, new { page, pageSize });
        _graphql = Page["items"]!.Select(p => (p["id"]!.Value<long>(), p["price"]!.Value<decimal>())).ToList();
        var rest = await SendAsync<PagedList<ProductItemMessageResponse>>($"Products?PageNumber={page}&PageSize={pageSize}");
        _rest = rest.Value.Items.Select(p => (p.Id, p.Price)).ToList();
    }

    [Then(@"as duas respostas trazem os mesmos produtos, na mesma ordem e com o mesmo preço")]
    public void ThenAsDuasRespostasTrazemOsMesmosProdutos()
    {
        _graphql.Should().NotBeEmpty();
        _graphql.Should().Equal(_rest);
    }
}
