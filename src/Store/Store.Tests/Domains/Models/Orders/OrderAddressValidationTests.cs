using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Store.Domain.Models.Orders;

namespace Store.Tests.Domains.Models.Orders
{
    [TestFixture, Category("Store")]
    public class OrderAddressValidationTests
    {


        [Test]
        public void Valid_order_address()
        {
            //Arrange's
            var address = new OrderAddress("street", "city", "state", "country", "zipcode");

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeTrue();
        }

        [Test]
        public void Invalid_order_address_street_null()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.Street, null)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_street_empty()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.Street, String.Empty)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_city_empty()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.City, String.Empty)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_city_null()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.City, null)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_state_empty()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.State, String.Empty)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_state_null()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.State, null)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_country_empty()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.Country, String.Empty)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_country_null()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.Country, null)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_zip_code_empty()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.ZipCode, String.Empty)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }

        [Test]
        public void Invalid_order_address_zip_code_null()
        {
            //Arrange's
            var address = Builder<OrderAddress>
                .CreateNew()
                .With(x => x.ZipCode, null)
                .Build();

            //Act
            var result = address.IsValid();

            //Assert's
            result.Should().BeFalse();
        }
    }
}