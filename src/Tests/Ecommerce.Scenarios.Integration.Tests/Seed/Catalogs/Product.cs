using System.Collections.Generic;
using Bogus;
using Catalog.Domain.Products;

namespace Ecommerce.Scenarios.Integration.Tests.Seed.Catalogs
{
    public static class Products
    {
        public static IEnumerable<Product> GetAll(int amountProduct)
        {
            return new Faker<Product>()
                .RuleFor(o => o.Name, f => f.Commerce.ProductName())
                .RuleFor(o => o.Price, f => double.Parse(f.Commerce.Price()))
                .RuleFor(o => o.PictureUri, f => "nothing")
                .RuleFor(o => o.Sku, f => "0001")
                .RuleFor(o => o.Description, f => f.Lorem.Text())
                .Generate(amountProduct);
        }
    }
}