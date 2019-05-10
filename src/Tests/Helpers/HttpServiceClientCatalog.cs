using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace Ecommerce.Integration.Tests.Helpers
{
    public class HttpServiceClientCatalog : BaseHttpServiceClient
    {
        public HttpServiceClientCatalog(TestServer client) : base(client, 62371)
        {
        }
    }
}
