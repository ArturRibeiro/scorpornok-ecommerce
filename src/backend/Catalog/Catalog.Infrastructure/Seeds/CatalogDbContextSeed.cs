namespace Catalog.Infrastructure.Seeds;

public static class CatalogDbContextSeed
{
    // Tênis da Netshoes (Seeds/products.json, recurso embutido). O Sku é o código do produto,
    // o mesmo nome do arquivo da imagem, servida pelo front-end em /catalog-imgs.
    private const string ProductsResource = "Catalog.Infrastructure.Seeds.products.json";

    public static async Task Seed(this ApplicationCatalogDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        if (await dbContext.Products.AnyAsync()) return;

        await dbContext.Products.AddRangeAsync(await GetProducts());
        await dbContext.SaveChangesAsync();
    }

    private static async Task<List<Product>> GetProducts()
    {
        await using var stream = typeof(CatalogDbContextSeed).Assembly.GetManifestResourceStream(ProductsResource)
            ?? throw new InvalidOperationException($"Recurso {ProductsResource} não encontrado.");

        var items = await JsonSerializer.DeserializeAsync<List<SeedProduct>>(stream)
            ?? throw new InvalidOperationException($"Recurso {ProductsResource} vazio.");

        return items
            .Select(item => new Product(item.Name, item.Brand, item.Sku, item.PictureUri, item.Price, item.Description))
            .ToList();
    }

    private sealed record SeedProduct(string Name, string Brand, string Sku, string PictureUri, decimal Price, string Description);
}
