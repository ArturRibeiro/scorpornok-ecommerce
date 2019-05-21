using Bogus;
using Catalog.Domain.Products;
using Catalog.Infrastructure;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.SeedDatas
{
    public class CatalogContextSeed
    {
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<bool> SeedAsync(CatalogContext context)
        {
            var productsFake = new Faker<Product>()
                .RuleFor(o => o.Name, f => f.Commerce.ProductName())
                .RuleFor(o => o.Description, f => f.Lorem.Text())
                .RuleFor(o => o.PictureUri, f => f.Image.Sports())
                .RuleFor(o => o.Sku, f => RandomString(20))
                .RuleFor(o => o.Price, f => decimal.Parse(f.Commerce.Price()));

            Product SeededProducts(int seed) => productsFake.UseSeed(seed).Generate();

            Enumerable.Range(1, 20)
               .Select(SeededProducts)
               .ToList()
               .ForEach(product => context.Products.Add(product));

            return await context.SaveEntitiesAsync(CancellationToken.None);
        }

        public static bool Start(CatalogContext context)
        {
            CatalogContextSeed seed = new CatalogContextSeed();
            var seedOk = seed.SeedAsync(context);
            seedOk.Wait();
            return seedOk.Result;
        }
    }
}
