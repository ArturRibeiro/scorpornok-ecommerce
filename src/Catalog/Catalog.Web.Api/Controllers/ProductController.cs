using Catalog.Queries.Products.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using Catalog.Queries.Products;
using Frameworker.Scorponok.AspNet.Mvc.Filters;
using Frameworker.Scorponok.AspNet.Mvc.ControllerBaseExtensions;

namespace Catalog.Web.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductQueries _productQueries;

        public ProductController(IProductQueries productQueries) => this._productQueries = productQueries;

        [HttpGet, Route("GetAllProducts")]
        [ProducesStatusCodeResponseType(HttpStatusCode.NotFound)]
        [ProducesStatusCodeResponseType(HttpStatusCode.InternalServerError)]
        [ProducesStatusCodeResponseType(HttpStatusCode.Unauthorized)]
        [ProducesStatusCodeResponseType(typeof(ProductItemMessageResponse[]),  HttpStatusCode.OK)]
        public async Task<IActionResult> GetAllProducts() 
            => this.Ok2(await _productQueries.GetAllProducts());
    }
}
