using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Shared.Code.Provider;

namespace Catalog.Queries.Products.Queries.Impl
{
    public class ProductQueries : IProductQueries
    {
        private readonly IDataConfigurationProvider _provider;

        public ProductQueries(IDataConfigurationProvider provider) => _provider = provider;

        public async Task<IEnumerable<ProductItemMessageResponse>> GetAllProducts()
        {
            await using (var connection = new SqliteConnection(_provider.ConnectionString))
            {
                var products = connection.Query<ProductItemMessageResponse>("SELECT CatalogId, Name, Sku, PictureUri, Price, Description FROM Products order by Name;");
                return products;
            };
           
        }
    }
}
