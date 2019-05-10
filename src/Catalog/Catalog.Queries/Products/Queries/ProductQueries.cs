using Bogus;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Queries.Products.Queries
{
    public class ProductQueries : IProductQueries
    {
        public async Task<ProductItemMessageResponse[]> GetAllProducts()
        {
            Faker<ProductItemMessageResponse> productItemFaker = new Faker<ProductItemMessageResponse>()
                .RuleFor(o => o.Id, f => f.IndexVariable++)
                .RuleFor(o => o.Name, f => f.Commerce.ProductName())
                .RuleFor(o => o.Description, f => f.Commerce.ProductAdjective())
                .RuleFor(o => o.Price, f => f.Commerce.Price());

            ProductItemMessageResponse SeededOrder(int seed) => productItemFaker.UseSeed(seed).Generate(); 

            var orders = Enumerable.Range(1, 5)
               .Select(SeededOrder)
               .ToList();

            return orders.ToArray();
        }
    }
}
