using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Frameworker.Scorponok.AspNet.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Order.Web.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        //[HttpGet, Route("GetAllProducts")]
        //[ProducesStatusCodeResponseType(HttpStatusCode.NotFound)]
        //[ProducesStatusCodeResponseType(HttpStatusCode.InternalServerError)]
        //[ProducesStatusCodeResponseType(HttpStatusCode.Unauthorized)]
        //[ProducesStatusCodeResponseType(typeof(CreateOrderCommand), HttpStatusCode.OK)]
        //public async Task<IActionResult> GetAllProducts() => this.Ok2(await _productQueries.GetAllProducts());
    }
}
