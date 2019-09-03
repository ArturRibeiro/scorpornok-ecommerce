using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Shared.Code.Commands;

namespace Store.Web.Api.App.Commands
{
    [DataContract]
    public class CreateOrderCommand : Message
    {
        [DataMember]
        public Guid UserId { get; set; }

        [DataMember] public OrderAddressMessageResponse Address { get; set; } = new OrderAddressMessageResponse();

        [DataMember]
        public IList<OrderItemMessageResponse> Items { get; set; } = new List<OrderItemMessageResponse>();
    }
}
