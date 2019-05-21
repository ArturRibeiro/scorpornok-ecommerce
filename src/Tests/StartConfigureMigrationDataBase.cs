using Catalog.Infrastructure;
using Ecommerce.Integration.Tests.SeedDatas;
using Frameworker.Scorponok.Tests.WebHost.Extensions;
using NUnit.Framework;
using Order.Infrastructure;

namespace Ecommerce.Integration.Tests
{
    public partial class Start
    {
        public Start MigrationDataBase()
        {

            _serverOrder.Host
                .ErasureDatabase<OrderContext>()
                .CreateDataBase<OrderContext>((context, services) => { });

            _serverCatalog.Host
                 .ErasureDatabase<CatalogContext>()
                 .CreateDataBase<CatalogContext>((context, services) =>
                 {
                     var result = CatalogContextSeed.Start(context);
                     Assert.IsTrue(result, "Ocorre algum erro ao aplicar o seed.");
                 });

            return this;
        }
    }
}

