using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Catalog.Queries.Products;
using Catalog.Web.Api;
using FluentAssertions;
using Frameworker.Integration.Tests;
using Frameworker.Integration.Tests.HttpClientExtensions;
using Frameworker.Scorponok.Reading.Database.Impl;
using NUnit.Framework;

namespace Ecommerce.Scenarios.Integration.Tests.Catalogs
{
    [TestFixture(Category = "Catalog")]
    public class CatalogTests : BaseWebApplicationFactory<Startup>
    {
        private readonly HttpClient _client;

        public CatalogTests()
        {
            _client = this.CreateClient();
            _client.Should().NotBeNull();
        }

        [TestCase("/api/v1/Product/GetAllProducts", TestName = "Return all products paged")]
        public async Task Run(string endPoint)
        {
            var response = await _client.GetAsync(endPoint);
            
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = response.PagedList<ProductItemMessageResponse>();
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().BeAssignableTo<PagedList<ProductItemMessageResponse>>();
            result.Data.Items.Should().HaveCountGreaterOrEqualTo(1);
        }
    }
}