
using Ecommerce.Integration.Tests.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using System.Net;
using FizzWare.NBuilder;
using Store.Web.Api.App.Commands;

namespace Ecommerce.Integration.Tests.Scenario.Orders
{
    [TestFixture, Category("Scenario")]
    public class OrderTests
    {
        private readonly HttpServiceClientOrder _client;

        public OrderTests()
        {
            _client = NativeInjectorBootStrapper.GetInstance<HttpServiceClientOrder>();
        }

        [Test, Order(1)]
        public async Task Create_order()
        {
            var command = new CreateOrderCommand()
            {
                Address = Builder<OrderAddressMessageResponse>.CreateNew().Build(),
                Items = Builder<OrderItemMessageResponse>.CreateListOfSize(1).Build().ToArray(),
                UserId = Guid.NewGuid()
            };

            command.UserId = Guid.NewGuid();

            var result = await _client.PostAsync(command, "create");

            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        //[Test, Order(2)]
        //public async Task Add_order_address()
        //{
        //    var command = new Orde() {
        //        City = "Rio de Janeiro",
        //        Country = "BRA",
        //        OrderId = Guid.NewGuid(),
        //        PhoneNumber = "01234567896",
        //        PostCode = "22222222",
        //        Street = "Av. Ayrton Senna",
        //        Number = "1234"
        //    };

        //    var result = await _client.PostAsync(command, "addAddress");

        //    result.StatusCode.Should().Be(HttpStatusCode.OK);
        //}

    }
}
