using Catalog.Queries.Products.Queries;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using Catalog.Queries.Products;
using Frameworker.Scorponok.AspNet.Mvc.Filters;
using Frameworker.Scorponok.AspNet.Mvc.ControllerBaseExtensions;
using Frameworker.Scorponok.AspNet.Mvc.Result;
using Frameworker.Scorponok.Reading.Database;

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
        [ProducesStatusCodeResponseType(typeof(ResultMessageResponse<IPagedList<ProductItemMessageResponse>>),  HttpStatusCode.OK)]
        public async Task<ResultMessageResponse<IPagedList<ProductItemMessageResponse>>> GetAllProducts([FromQuery] PagingModel pagingModel)
        {
            return this.Ok2(await _productQueries.GetAllProducts(pagingModel.pageNumber, pagingModel.PageSize));
        }
    }
    
    public class PagingModel  
    {  
        const int maxPageSize = 20;  
  
        public int pageNumber { get; set; } = 1;  
  
        public int PageSize { get; set; } = 10;  
  
        // public int pageSize  
        // {  
        //
        //     get { return _pageSize; }  
        //     set  
        //     {  
        //         _pageSize = (value > maxPageSize) ? maxPageSize : value;  
        //     }  
        // }  
    }  
}
