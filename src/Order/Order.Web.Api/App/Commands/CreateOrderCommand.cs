using System;
using Shared.Code.Commands;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Order.Web.Api.App.Commands
{
    [DataContract]
    public class CreateOrderCommand : Message
    {
        [DataMember]
        public Guid UserId { get; set; }
    }
}
