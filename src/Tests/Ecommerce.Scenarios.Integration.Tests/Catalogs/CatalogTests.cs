// using System.Linq;
// using System.Net;
// using System.Net.Http;
// using System.Threading.Tasks;
// using Catalog.Infrastructure;
// using Catalog.Queries.Products;
// using Catalog.Web.Api;
// using FluentAssertions;
// using Frameworker.Integration.Tests.HttpClientExtensions;
// using Frameworker.Integration.Tests.WebApplicationFactorys.Postgres;
// using Frameworker.Scorponok.Reading.Database.Impl;
// using Microsoft.Extensions.DependencyInjection;
// using NUnit.Framework;
//
// namespace Ecommerce.Scenarios.Integration.Tests.Catalogs
// {
//     [TestFixture(Category = "Catalog")]
//     public class CatalogTests
//     {
//         private readonly WebApplicationFactoryPostgres<Startup, ApplicationCatalogDbContext> _factory;
//         private readonly HttpClient _client;
//
//         public CatalogTests()
//         {
//             _factory = new WebApplicationFactoryPostgres<Startup, ApplicationCatalogDbContext>();
//             _client = _factory.CreateClient();
//              _client.Should().NotBeNull();
//         }
//
//         
//         [TestCase("/api/v1/Product/GetAllProducts?pageSize=5&pageNumber=1", TestName = "Return all products paged")]
//         [TestCase("/api/v1/Product/GetAllProducts?pageSize=5&pageNumber=2", TestName = "Return all products paged")]
//         public async Task Run(string endPoint)
//         {
//             var response = await _client.GetAsync(endPoint);
//             
//             response.StatusCode.Should().Be(HttpStatusCode.OK);
//             var applicationCatalogDbContext = _factory.Services.GetService<ApplicationCatalogDbContext>();
//             var count = applicationCatalogDbContext.Products.Count();
//             var result = response.PagedList<ProductItemMessageResponse>();
//             result.Should().NotBeNull();
//             result.Success.Should().BeTrue();
//             result.Data.Should().BeAssignableTo<PagedList<ProductItemMessageResponse>>();
//             result.Data.Items.Should().HaveCountGreaterOrEqualTo(1);
//             result.Data.PageInfo.Should().NotBeNull();
//         }
//     }
// }