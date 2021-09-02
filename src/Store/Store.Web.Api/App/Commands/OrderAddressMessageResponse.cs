using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Shared.Code.Commands;

namespace Store.Web.Api.App.Commands
{
    [DataContract]
    public class OrderAddressMessageResponse
    {
        [DataMember, Required]
        public string Street { get; private set; }

        [DataMember(Name = "city"), Required]
        public string City { get; private set; }

        [DataMember, Required]
        public string State { get; private set; }

        [DataMember, Required]
        public string Country { get; private set; }

        [DataMember, Required]
        public string ZipCode { get; private set; }
    }
}
