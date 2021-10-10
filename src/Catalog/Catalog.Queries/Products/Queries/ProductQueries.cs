using Dapper;
using Shared.Code.Provider;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Bogus;
using Catalog.Infrastructure;
using Microsoft.Data.Sqlite;

namespace Catalog.Queries.Products.Queries
{
    public class ProductQueries : IProductQueries
    {
        private readonly IDataConfigurationProvider _provider;
        private readonly ApplicationCatalogDbContext _context;

        public ProductQueries(IDataConfigurationProvider provider, ApplicationCatalogDbContext context)
        {
            _provider = provider;
            //_context = context;
        }

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
