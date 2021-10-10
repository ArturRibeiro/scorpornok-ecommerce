using Catalog.Infrastructure;
using Ecommerce.Integration.Tests.SeedDatas;
using Frameworker.Scorponok.Tests.WebHost.Extensions;
using NUnit.Framework;
using Store.Infrastructure;

namespace Ecommerce.Integration.Tests
{
    public partial class Start
    {
        public Start MigrationDataBase()
        {
            _serverOrder.Host
                .ErasureDatabase<OrderContext>()
                .CreateDataBase<OrderContext>((context, services) => { });

            // _serverCatalog.Host
            //      .CreateDataBase<CatalogContext>((context, services) => { CatalogContextSeed.Start(context); });

            return this;
        }
    }
}

