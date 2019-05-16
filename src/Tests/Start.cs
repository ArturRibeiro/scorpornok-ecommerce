using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;

namespace Ecommerce.Integration.Tests
{
    [SetUpFixture]
    public partial class Start
    {
        private TestServer _serverPayment;
        private TestServer _serverOrder;
        private TestServer _serverCatalog;

        [OneTimeSetUp]
        public void Setup()
        {
            this
                .ConfigureWebHostBuilder()
                .CreateDependencyInjection()
                .MigrationDataBase();
        }
    }
}