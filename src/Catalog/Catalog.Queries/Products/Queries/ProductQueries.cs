using Dapper;
using Shared.Code.Provider;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Catalog.Queries.Products.Queries
{
    public class ProductQueries : IProductQueries
    {
        private readonly IDataConfigurationProvider _provider;

        public ProductQueries(IDataConfigurationProvider provider)
            => _provider = provider;

        public async Task<IEnumerable<ProductItemMessageResponse>> GetAllProducts()
        {
            using (var connection = new SqlConnection(_provider.ConnectionString))
            {
                connection.Open();

                return await connection.QueryAsync<ProductItemMessageResponse>(@"SELECT [CatalogId]
                                                                                      ,[Name]
                                                                                      ,[Sku]
                                                                                      ,[PictureUri]
                                                                                      ,[Price]
                                                                                      ,[Description]
                                                                                 FROM [dbo].[Products] ORDER BY [Name]");
            }
        }
    }
}
