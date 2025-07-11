namespace Ecommerce.Scenarios.Integration.Spec.Tests.Seed.Catalogs
{
    public static class Products
    {
        public static IEnumerable<Product> CreateProducts(int amountProduct)
        {
            return new Faker<Product>("pt_BR")
                .RuleFor(o => o.Name, f => f.Commerce.ProductName())
                .RuleFor(o => o.Price, f => f.Finance.Amount())
                .RuleFor(o => o.PictureUri, f => f.Image.PicsumUrl())
                .RuleFor(o => o.Sku, f => f.Random.Replace("*********"))
                .RuleFor(o => o.Description, f => f.Commerce.ProductDescription())
                .Generate(amountProduct);
        }
    }
}