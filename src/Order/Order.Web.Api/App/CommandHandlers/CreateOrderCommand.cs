using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Web.Api.App.CommandHandlers
{
    public class CreateOrderCommand : IRequest<int>
    {
        public Guid UserId { get; set; }

        public AddOrderAddressCommand Address { get; set; }
    }
}
