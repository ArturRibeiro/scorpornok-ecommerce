
using Ecommerce.Integration.Tests.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Order.Web.Api.App.Commands;

namespace Ecommerce.Integration.Tests.Scenario.Orders
{
    [TestFixture]
    public class OrderTests
    {
        private readonly HttpServiceClientOrder _client;

        public OrderTests()
        {
            _client = NativeInjectorBootStrapper.GetInstance<HttpServiceClientOrder>();
        }

        [Test]
        public async Task Create_order()
        {
            var command = new CreateOrderCommand() { };

            var result = await _client.PostAsync(command, "createOrder");
        }

    }
}
