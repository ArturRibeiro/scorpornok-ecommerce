using Catalog.Queries.Products.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.Web.Api.Controllers
{
    [Route("api/v1/[controller]"), ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductQueries _productQueries;

        public ProductController(IProductQueries productQueries)
        {
            this._productQueries = productQueries;
        }

        [HttpGet, Route("GetAllProducts")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> GetAllProducts() => Ok(await _productQueries.GetAllProducts());
    }
}
