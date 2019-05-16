using Ecommerce.Integration.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Frameworker.Scorponok.Tests.WebHost.Extensions;
using Microsoft.AspNetCore.Hosting;

namespace Ecommerce.Integration.Tests
{
    public partial class Start
    {
        public Start MigrationDataBase()
        {

            this._serverOrder
                .Host
                .ErasureDatabase<Order.Infrastructure.OrderContext>((context, services) => { })
                .CreateDataBase<Order.Infrastructure.OrderContext>((context, services) => {

                });

            return this;
        }
    }
}
