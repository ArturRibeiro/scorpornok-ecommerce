using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Web.Api.App.CommandHandlers
{
    public class AddOrderAddressCommand
    {
        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string PostCode { get; set; }

        public int PhoneNumber { get; set; }

        [Required]
        public string Country { get; set; }
    }
}
