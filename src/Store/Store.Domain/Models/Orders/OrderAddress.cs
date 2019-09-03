using System.Collections.Generic;
using Shared.Code.Models;
using Store.Domain.Models.Orders.Validations;

namespace Store.Domain.Models.Orders
{
    /// <summary>
    /// Endereço de entrega do pedido
    /// </summary>
    public class OrderAddress : ValueObject
    {
        /// <summary>
        /// 
        /// </summary>
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string ZipCode { get; private set; }

        //EF
        protected OrderAddress() { }

        private OrderAddress(string street, string city, string state, string country, string zipcode)
        {
            Street = street;
            City = city;
            State = state;
            Country = country;
            ZipCode = zipcode;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            // Using a yield return statement to return each element one at a time
            yield return Street;
            yield return City;
            yield return State;
            yield return Country;
            yield return ZipCode;
        }

        public bool IsValid()
            => new OrderAddressValidation().Validate(this).IsValid;

        #region Factory

        public class Factory
        {
            public static OrderAddress Create(string street, string city, string state, string country, string zipcode)
                => new OrderAddress()

                {
                    Street = street,
                    City = city,
                    State = state,
                    Country = country,
                    ZipCode = zipcode
                };
        }
    }

    #endregion
}

