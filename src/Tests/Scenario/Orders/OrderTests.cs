
using Ecommerce.Integration.Tests.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;

namespace Ecommerce.Integration.Tests.Scenario.Orders
{
    [TestFixture]
    public class OrderTests
    {
        private readonly BaseHttpServiceClient _client;

        public OrderTests()
        {
            _client = NativeInjectorBootStrapper.GetInstance<HttpServiceClientOrder>();
        }

        [Test]
        public async Task GetValues()
        {
            var result = await _client.GetAsync("values");
            result.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
