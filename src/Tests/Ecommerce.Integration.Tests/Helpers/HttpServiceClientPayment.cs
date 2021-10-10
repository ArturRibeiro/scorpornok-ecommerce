using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;

namespace Ecommerce.Integration.Tests.Helpers
{
    public class HttpServiceClientPayment : BaseHttpServiceClient
    {
        public HttpServiceClientPayment(TestServer client) : base(client, 62373)
        {
        }
    }
}
