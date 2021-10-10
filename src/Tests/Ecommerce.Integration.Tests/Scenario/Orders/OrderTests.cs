
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
    }
}
