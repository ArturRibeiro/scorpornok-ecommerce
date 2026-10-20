namespace Catalog.Web.Api.GraphQL;

/// <summary>Produto do catálogo no GraphQL: os mesmos campos do REST (<see cref="ProductItemMessageResponse"/>).</summary>
public record Product(long Id, string Name, string Sku, string PictureUri, decimal Price, string Description)
{
    public static Product From(ProductItemMessageResponse p)
        => new(p.Id, p.Name, p.Sku, p.PictureUri, p.Price, p.Description);
}

/// <summary>Uma página do catálogo, no mesmo formato do <c>GET /Products</c>.</summary>
public record ProductPage(IReadOnlyList<Product> Items, ProductPageInfo PageInfo);

public record ProductPageInfo(int CurrentPage, int PageSize, int TotalItems, int TotalPages, bool HasPreviousPage,
    bool HasNextPage);

/// <summary>
/// Consultas do Catalog como subgraph do gateway (Fusion), sobre as mesmas consultas do REST: as duas APIs
/// devolvem os mesmos produtos, na mesma ordem, com as mesmas regras de página.
/// </summary>
[QueryType]
public static partial class ProductQueries
{
    // Páginas numeradas, como na home. Os mesmos ajustes do REST (PagingModel): página a partir de 1 e
    // tamanho de 1 a 20, sem erro para valores fora disso.
    public static async Task<ProductPage> GetProducts(int? page, int? pageSize, [Service] IProductQueries queries)
        => ToPage(await queries.GetAllProducts(new PagingModel { PageNumber = page, PageSize = pageSize }));

    // Busca no nome, na marca e na descrição (todas as palavras, sem diferenciar maiúsculas e acentos), com
    // marcas e faixa de preço e as mesmas páginas do products; os do nome vêm primeiro. Sem nenhum filtro,
    // devolve o catálogo inteiro. As sugestões da loja usam a mesma consulta com pageSize 5.
    public static async Task<ProductPage> SearchProducts(string? term, IReadOnlyList<string>? brands,
        decimal? minPrice, decimal? maxPrice, int? page, int? pageSize, [Service] IProductQueries queries)
        => ToPage(await queries.SearchProducts(
            new ProductSearch { Term = term, Brands = brands, MinPrice = minPrice, MaxPrice = maxPrice },
            new PagingModel { PageNumber = page, PageSize = pageSize }));

    // As marcas do catálogo, em ordem alfabética, para o filtro da busca.
    public static async Task<IReadOnlyList<string>> GetProductBrands([Service] IProductQueries queries)
        => await queries.GetBrands();

    // Nulo para id inexistente, sem erro. Como lookup, o gateway também o usa para resolver um produto
    // citado por outro subgraph (ex.: os itens de um pedido, numa change futura).
    [Lookup]
    public static async Task<Product?> GetProductById(long id, [Service] IProductQueries queries)
        => await queries.GetProductById(id) is { } p ? Product.From(p) : null;

    private static ProductPage ToPage(IPagedList<ProductItemMessageResponse> result)
    {
        var info = result.PageInfo;
        return new ProductPage(result.Items.Select(Product.From).ToList(),
            new ProductPageInfo(info.CurrentPage, info.PageSize, info.TotalItems, info.TotalPages,
                info.HasPreviousPage, info.HasNextPage));
    }
}
