using Dapper;
using Shared.Code.Provider;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Bogus;

namespace Catalog.Queries.Products.Queries
{
    public class ProductQueries : IProductQueries
    {
        private readonly IDataConfigurationProvider _provider;

        public ProductQueries(IDataConfigurationProvider provider)
            => _provider = provider;

        public async Task<IEnumerable<ProductItemMessageResponse>> GetAllProducts()
        {
            var faker = new Faker<ProductItemMessageResponse>()
                .RuleFor(o => o.Name, f => f.Commerce.ProductName())
                .RuleFor(o => o.Price, f => decimal.Parse(f.Commerce.Price()))
                .RuleFor(o => o.PictureUri, f => "nothing");
            
            return faker.Generate(20);
        }
    }
}
