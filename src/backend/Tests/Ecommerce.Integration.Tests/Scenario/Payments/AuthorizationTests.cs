using FluentAssertions;
using Ecommerce.Integration.Tests.Helpers;
using Gateway.Payment.Web.Api.App.eRede.Commands;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Scenario.Payments
{
    [TestFixture(CardOperator.eRede), Category("Scenario")]
    public class AuthorizationTests
    {
        private readonly BaseHttpServiceClient client;

        public AuthorizationTests(CardOperator @operator)
        {
            client = NativeInjectorBootStrapper.GetInstance<HttpServiceClientPayment>();
            client.AddHeader("CardOperator", @operator);
        }

        [Test]
        public async Task Autorizacao()
        {

            var command = new AuthorizationCommand()
            {
                Amount = 1000,
                Caputre = false,
                CardHolderName = "Artur Ribeiro",
                CardNumber = "01234567890123",
                SecurityCode = "12",
                DistributorAffiliation = "12345",
                ExpirationMonth = 12,
                ExpirationYear = 2026,
                Installments = 0,
                Origin = Origin.eRede,
                Reference = "012345679",
                SoftDescriptor = "teste",
                Subscription = false,
                TransactionType = TransactionType.Credit
            };


            var result = await client.PostAsync(command, "Authorization/Authorize");


            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
