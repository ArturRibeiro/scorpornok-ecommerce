using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Order.Web.Api.App.Commands
{
    [DataContract]
    public class AddOrderAddressCommand
    {
        [DataMember, Required]
        public string City { get; set; }

        [DataMember, Required]
        public string Street { get; set; }

        [DataMember, Required]
        public string PostCode { get; set; }

        [DataMember]
        public int PhoneNumber { get; set; }

        [DataMember, Required]
        public string Country { get; set; }
    }
}
