using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Queries.Products;
using Frameworker.Scorponok.AspNet.Mvc.Result;
using Microsoft.AspNetCore.TestHost;

namespace Ecommerce.Integration.Tests.Helpers
{
    public class HttpServiceClientCatalog : BaseHttpServiceClient
    {
        private static int port = 62371;

        public HttpServiceClientCatalog(TestServer client) : base(client, port)
        {
        }

        public async Task<ResultMessageResponseTest<ProductItemMessageResponse[]>> GetAllProducts()
        {
            return await this.GetAsync<ProductItemMessageResponse[]>("Product/GetAllProducts");
        }
    }
}
