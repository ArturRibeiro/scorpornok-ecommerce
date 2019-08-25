using System.Threading.Tasks;
using Catalog.Queries.Products;
using Microsoft.AspNetCore.TestHost;

namespace Ecommerce.Integration.Tests.Helpers
{
    public class HttpServiceClientCatalog : BaseHttpServiceClient
    {
        private static int port = 55365;

        public HttpServiceClientCatalog(TestServer client) : base(client, port)
        {
        }

        public async Task<ResultMessageResponseTest<ProductItemMessageResponse[]>> GetAllProducts()
            => await this.GetAsync<ProductItemMessageResponse[]>("Product/GetAllProducts");
    }
}
