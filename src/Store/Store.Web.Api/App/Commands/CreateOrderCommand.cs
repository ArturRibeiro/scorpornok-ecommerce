using System;
using System.Runtime.Serialization;
using Shared.Code.Commands;

namespace Store.Web.Api.App.Commands
{
    [DataContract]
    public class CreateOrderCommand : Message
    {
        [DataMember]
        public Guid UserId { get; set; }
    }
}
