
using Ecommerce.Integration.Tests.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Catalog.Queries.Products;
using System.Threading.Tasks;
using FluentAssertions;

namespace Ecommerce.Integration.Tests.Scenario.Catalog
{
    [TestFixture]
    public class CatalogTests
    {
        private readonly HttpServiceClientCatalog _catalogoServiceClient;

        public CatalogTests()
        {
            _catalogoServiceClient = NativeInjectorBootStrapper.GetInstance<HttpServiceClientCatalog>();
        }

        [Test]
        public async Task GetAllProducts()
        {
            var result = await _catalogoServiceClient.GetAllProducts();
            result.Success.Should().BeTrue();
            result.Errors.Should().BeNull();
            result.Should().BeAssignableTo<ResultMessageResponseTest<ProductItemMessageResponse[]>>();
        }
    }
}
