
using Ecommerce.Integration.Tests.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace Ecommerce.Integration.Tests.Scenario.Catalog
{
    [TestFixture]
    public class CatalogTests
    {
        private readonly BaseHttpServiceClient _client;

        public CatalogTests()
        {
            _client = NativeInjectorBootStrapper.GetInstanceHttpServiceClient<HttpServiceClientCatalog>();
        }

        [Test]
        public async Task GetValues()
        {
            var result = await _client.GetAsync("values");
            result.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
