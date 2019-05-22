using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Frameworker.Scorponok.AspNet.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Order.Web.Api.App.Commands;
using Shared.Code.Bus;
using Frameworker.Scorponok.AspNet.Mvc.ControllerBaseExtensions;

namespace Order.Web.Api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IMediatorHandler _mediator;

        public OrdersController(IMediatorHandler mediator)
            => _mediator = mediator;

        [HttpPost, Route("create")]
        [ProducesStatusCodeResponseType(HttpStatusCode.NotFound)]
        [ProducesStatusCodeResponseType(HttpStatusCode.InternalServerError)]
        [ProducesStatusCodeResponseType(HttpStatusCode.Unauthorized)]
        [ProducesStatusCodeResponseType(typeof(CreateOrderCommand), HttpStatusCode.OK)]
        public async Task<IActionResult> AddOrder(CreateOrderCommand command)
        {
            await _mediator.Send(command);

            return this.Ok();
        }


        [HttpPost, Route("addAddress")]
        [ProducesStatusCodeResponseType(HttpStatusCode.NotFound)]
        [ProducesStatusCodeResponseType(HttpStatusCode.InternalServerError)]
        [ProducesStatusCodeResponseType(HttpStatusCode.Unauthorized)]
        [ProducesStatusCodeResponseType(typeof(OrderAddressCommand), HttpStatusCode.OK)]
        public async Task<IActionResult> AddAddress(OrderAddressCommand command)
        {
            await _mediator.Send(command);

            return this.Ok();
        }
    }
}
