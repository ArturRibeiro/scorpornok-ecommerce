namespace Catalog.Web.Api.WebApplicationExtensions;

public static class ApplicationCatalogDbContextExtensions
{
    public static async Task Seed(this ApplicationCatalogDbContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.Products.AddRangeAsync(GetProducts());
        await dbContext.SaveChangesAsync();
    }

    private static List<Product> GetProducts()
    {
        return
        [
            new Product("Product 1", "SKU-0001", "https://picsum.photos/seed/1/300/300", 19.99m, "Description for product 1"),
            new Product("Product 2", "SKU-0002", "https://picsum.photos/seed/2/300/300", 29.99m, "Description for product 2"),
            new Product("Product 3", "SKU-0003", "https://picsum.photos/seed/3/300/300", 39.99m, "Description for product 3"),
            new Product("Product 4", "SKU-0004", "https://picsum.photos/seed/4/300/300", 49.99m, "Description for product 4"),
            new Product("Product 5", "SKU-0005", "https://picsum.photos/seed/5/300/300", 59.99m, "Description for product 5"),
            new Product("Product 6", "SKU-0006", "https://picsum.photos/seed/6/300/300", 69.99m, "Description for product 6"),
            new Product("Product 7", "SKU-0007", "https://picsum.photos/seed/7/300/300", 79.99m, "Description for product 7"),
            new Product("Product 8", "SKU-0008", "https://picsum.photos/seed/8/300/300", 89.99m, "Description for product 8"),
            new Product("Product 9", "SKU-0009", "https://picsum.photos/seed/9/300/300", 99.99m, "Description for product 9"),
            new Product("Product 10", "SKU-0010", "https://picsum.photos/seed/10/300/300", 109.99m, "Description for product 10"),
            new Product("Product 11", "SKU-0011", "https://picsum.photos/seed/11/300/300", 119.99m, "Description for product 11"),
            new Product("Product 12", "SKU-0012", "https://picsum.photos/seed/12/300/300", 129.99m, "Description for product 12"),
            new Product("Product 13", "SKU-0013", "https://picsum.photos/seed/13/300/300", 139.99m, "Description for product 13"),
            new Product("Product 14", "SKU-0014", "https://picsum.photos/seed/14/300/300", 149.99m, "Description for product 14"),
            new Product("Product 15", "SKU-0015", "https://picsum.photos/seed/15/300/300", 159.99m, "Description for product 15"),
            new Product("Product 16", "SKU-0016", "https://picsum.photos/seed/16/300/300", 169.99m, "Description for product 16"),
            new Product("Product 17", "SKU-0017", "https://picsum.photos/seed/17/300/300", 179.99m, "Description for product 17"),
            new Product("Product 18", "SKU-0018", "https://picsum.photos/seed/18/300/300", 189.99m, "Description for product 18"),
            new Product("Product 19", "SKU-0019", "https://picsum.photos/seed/19/300/300", 199.99m, "Description for product 19"),
            new Product("Product 20", "SKU-0020", "https://picsum.photos/seed/20/300/300", 209.99m, "Description for product 20"),
            new Product("Product 21", "SKU-0021", "https://picsum.photos/seed/21/300/300", 219.99m, "Description for product 21"),
            new Product("Product 22", "SKU-0022", "https://picsum.photos/seed/22/300/300", 229.99m, "Description for product 22"),
            new Product("Product 23", "SKU-0023", "https://picsum.photos/seed/23/300/300", 239.99m, "Description for product 23"),
            new Product("Product 24", "SKU-0024", "https://picsum.photos/seed/24/300/300", 249.99m, "Description for product 24"),
            new Product("Product 25", "SKU-0025", "https://picsum.photos/seed/25/300/300", 259.99m, "Description for product 25"),
            new Product("Product 26", "SKU-0026", "https://picsum.photos/seed/26/300/300", 269.99m, "Description for product 26"),
            new Product("Product 27", "SKU-0027", "https://picsum.photos/seed/27/300/300", 279.99m, "Description for product 27"),
            new Product("Product 28", "SKU-0028", "https://picsum.photos/seed/28/300/300", 289.99m, "Description for product 28"),
            new Product("Product 29", "SKU-0029", "https://picsum.photos/seed/29/300/300", 299.99m, "Description for product 29"),
            new Product("Product 30", "SKU-0030", "https://picsum.photos/seed/30/300/300", 309.99m, "Description for product 30"),
            new Product("Product 31", "SKU-0031", "https://picsum.photos/seed/31/300/300", 319.99m, "Description for product 31"),
            new Product("Product 32", "SKU-0032", "https://picsum.photos/seed/32/300/300", 329.99m, "Description for product 32"),
            new Product("Product 33", "SKU-0033", "https://picsum.photos/seed/33/300/300", 339.99m, "Description for product 33"),
            new Product("Product 34", "SKU-0034", "https://picsum.photos/seed/34/300/300", 349.99m, "Description for product 34"),
            new Product("Product 35", "SKU-0035", "https://picsum.photos/seed/35/300/300", 359.99m, "Description for product 35"),
            new Product("Product 36", "SKU-0036", "https://picsum.photos/seed/36/300/300", 369.99m, "Description for product 36"),
            new Product("Product 37", "SKU-0037", "https://picsum.photos/seed/37/300/300", 379.99m, "Description for product 37"),
            new Product("Product 38", "SKU-0038", "https://picsum.photos/seed/38/300/300", 389.99m, "Description for product 38"),
            new Product("Product 39", "SKU-0039", "https://picsum.photos/seed/39/300/300", 399.99m, "Description for product 39"),
            new Product("Product 40", "SKU-0040", "https://picsum.photos/seed/40/300/300", 409.99m, "Description for product 40")
        ];
    }
}