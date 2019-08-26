using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Shared.Code.Commands;

namespace Store.Web.Api.App.Commands
{
    [DataContract]
    public class OrderAddressCommand : Message
    {
        [DataMember, Required]
        public Guid OrderId { get; set; }

        /// <summary>
        /// Cidade do endereço
        /// </summary>
        [DataMember, Required]
        public string City { get; set; }

        /// <summary>
        /// Nome da rua do endereço.
        /// </summary>
        [DataMember, Required]
        public string Street { get; set; }

        /// <summary>
        /// CEP do endereço
        /// </summary>
        [DataMember, Required]
        public string PostCode { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Pais do endereço
        /// </summary>
        [DataMember, Required]
        public string Country { get; set; }

        /// <summary>
        /// Número do endereço.
        /// </summary>
        [DataMember, Required]
        public string Number { get; set; }
    }
}
