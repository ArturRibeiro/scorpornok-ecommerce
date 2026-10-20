namespace Ecommerce.Scenarios.Integration.Spec.Tests.Steps;

// Busca de produtos pelo /graphql do Catalog (spec product-search). O esperado sai do próprio catálogo,
// lido inteiro pelo REST, para os cenários não dependerem das contagens da carga.
[Binding]
public class CatalogSearchStepDefinitions : StepBase
{
    private const string SearchQuery =
        "query($term: String, $brands: [String!], $minPrice: Decimal, $maxPrice: Decimal, $page: Int, $pageSize: Int) { " +
        "searchProducts(term: $term, brands: $brands, minPrice: $minPrice, maxPrice: $maxPrice, page: $page, pageSize: $pageSize) { " +
        "pageInfo { totalItems hasNextPage } items { id name price } } }";

    private JToken _page = null!;
    private string[] _brands = [];
    private (decimal Min, decimal Max) _range;

    private IEnumerable<JToken> Items => _page["items"]!;

    private async Task SearchAsync(object variables)
    {
        var response = await GraphQLAsync(SearchQuery, variables);
        response["errors"].Should().BeNull();
        _page = response["data"]!["searchProducts"]!;
    }

    // O catálogo inteiro pelo REST, página a página (até 20 por página).
    private async Task<List<ProductItemMessageResponse>> AllProductsAsync()
    {
        var all = new List<ProductItemMessageResponse>();
        for (var page = 1; ; page++)
        {
            var result = await SendAsync<PagedList<ProductItemMessageResponse>>($"Products?PageNumber={page}&PageSize=20");
            all.AddRange(result.Value.Items);
            if (!result.Value.PageInfo.HasNextPage) return all;
        }
    }

    [When(@"busco pelo GraphQL ""(.*)""")]
    public async Task WhenBuscoPeloGraphQL(string term) => await SearchAsync(new { term, pageSize = 20 });

    [Then(@"os resultados trazem o produto ""(.*)""")]
    public void ThenOsResultadosTrazemOProduto(string name)
        => Items.Select(p => p["name"]!.Value<string>()).Should().Contain(name);

    [Then(@"o primeiro resultado tem ""(.*)"" e ""(.*)"" no nome")]
    public void ThenOPrimeiroResultadoTem(string first, string second)
    {
        var name = Normalize(Items.First()["name"]!.Value<string>()!);
        name.Should().Contain(first).And.Contain(second);
    }

    // Minúsculas e sem acentos, como a busca compara.
    private static string Normalize(string text)
        => new(text.ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                        != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray());

    // A marca está no nome de todos os produtos da carga, então nome e descrição cobrem os três campos.
    [Then(@"recebo todos os produtos que citam ""(.*)"" no nome ou na descrição")]
    public async Task ThenReceboTodosOsProdutosQueCitam(string word)
    {
        var expected = (await AllProductsAsync()).Count(p => Normalize(p.Name + " " + p.Description).Contains(Normalize(word)));
        expected.Should().BePositive().And.BeLessThan(281);
        _page["pageInfo"]!["totalItems"]!.Value<int>().Should().Be(expected);
    }

    // As sugestões da loja são a mesma busca, com 5 por página.
    [When(@"busco pelo GraphQL as 5 sugestões de ""(.*)""")]
    public async Task WhenBuscoAsSugestoes(string term) => await SearchAsync(new { term, pageSize = 5 });

    [Then(@"as sugestões trazem produtos que citam ""(.*)"" na descrição")]
    public async Task ThenAsSugestoesTrazemProdutosQueCitam(string word)
    {
        Items.Should().HaveCount(5);
        var byId = (await AllProductsAsync()).ToDictionary(p => p.Id);
        Items.Should().OnlyContain(p => Normalize(byId[p["id"]!.Value<long>()].Description).Contains(word));
    }

    private List<string> _names = [];

    [When(@"busco pelo GraphQL todas as páginas de ""(.*)""")]
    public async Task WhenBuscoTodasAsPaginas(string term)
    {
        _names = [];
        for (var page = 1; ; page++)
        {
            await SearchAsync(new { term, page, pageSize = 20 });
            _names.AddRange(Items.Select(p => p["name"]!.Value<string>()!));
            if (!_page["pageInfo"]!["hasNextPage"]!.Value<bool>()) break;
        }
    }

    [Then(@"os produtos com ""(.*)"" no nome vêm antes dos que só o citam na descrição")]
    public void ThenOsProdutosComONomeVemAntes(string word)
    {
        var inName = _names.Select(n => Normalize(n).Contains(word)).ToList();
        var firstWithout = inName.IndexOf(false);
        firstWithout.Should().BePositive("há produtos com a palavra no nome e outros só na descrição");
        inName.Skip(firstWithout).Should().OnlyContain(found => !found);
    }

    [Then(@"nenhum produto é encontrado")]
    public void ThenNenhumProdutoEEncontrado()
    {
        Items.Should().BeEmpty();
        _page["pageInfo"]!["totalItems"]!.Value<int>().Should().Be(0);
    }

    [When(@"busco pelo GraphQL as marcas ""(.*)"" e ""(.*)""")]
    public async Task WhenBuscoPeloGraphQLAsMarcas(string first, string second)
    {
        _brands = [first, second];
        await SearchAsync(new { brands = _brands, pageSize = 20 });
    }

    [Then(@"recebo todos os produtos dessas marcas, e só eles")]
    public async Task ThenReceboTodosOsProdutosDessasMarcas()
    {
        // A marca está no nome de todos os produtos da carga ("Tênis Nike ...").
        var expected = (await AllProductsAsync()).Count(p => _brands.Any(b => p.Name.Contains(b)));
        expected.Should().BePositive();
        _page["pageInfo"]!["totalItems"]!.Value<int>().Should().Be(expected);
        Items.Should().OnlyContain(p => _brands.Any(b => p["name"]!.Value<string>()!.Contains(b)));
    }

    [When(@"busco pelo GraphQL os produtos de (.*) a (.*)")]
    public async Task WhenBuscoPeloGraphQLOsProdutosDeA(decimal minPrice, decimal maxPrice)
    {
        _range = (minPrice, maxPrice);
        await SearchAsync(new { minPrice, maxPrice, pageSize = 20 });
    }

    [Then(@"recebo todos os produtos nessa faixa, inclusive os de (.*)")]
    public async Task ThenReceboTodosOsProdutosNessaFaixa(decimal limit)
    {
        var expected = (await AllProductsAsync()).Count(p => p.Price >= _range.Min && p.Price <= _range.Max);
        _page["pageInfo"]!["totalItems"]!.Value<int>().Should().Be(expected);
        Items.Should().Contain(p => p["price"]!.Value<decimal>() == limit);
        Items.Should().OnlyContain(p => p["price"]!.Value<decimal>() >= _range.Min && p["price"]!.Value<decimal>() <= _range.Max);
    }

    [When(@"busco pelo GraphQL sem filtros com (\d+) por página")]
    public async Task WhenBuscoPeloGraphQLSemFiltros(int pageSize) => await SearchAsync(new { pageSize });

    [Then(@"a página da busca traz (\d+) produtos")]
    public void ThenAPaginaDaBuscaTraz(int count) => Items.Should().HaveCount(count);

    private List<string> _catalogBrands = [];

    [When(@"consulto as marcas pelo GraphQL")]
    public async Task WhenConsultoAsMarcasPeloGraphQL()
    {
        var response = await GraphQLAsync("{ productBrands }");
        response["errors"].Should().BeNull();
        _catalogBrands = response["data"]!["productBrands"]!.Values<string>().Select(b => b!).ToList();
    }

    [Then(@"recebo as marcas do catálogo, sem repetição e em ordem alfabética")]
    public void ThenReceboAsMarcasDoCatalogo()
    {
        _catalogBrands.Should().NotBeEmpty().And.OnlyHaveUniqueItems();
        _catalogBrands.Should().BeInAscendingOrder(StringComparer.Ordinal);
        _catalogBrands.Should().Contain(["Nike", "Adidas", "New Balance"]);
    }
}
