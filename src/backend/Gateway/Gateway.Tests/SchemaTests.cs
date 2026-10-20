extern alias catalog;
extern alias customers;
extern alias orders;
extern alias payments;
using System.IO.Compression;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using CatalogTypes = catalog::Microsoft.Extensions.DependencyInjection.ApiTypesRequestExecutorBuilderExtensions;
using CustomersTypes = customers::Microsoft.Extensions.DependencyInjection.ApiTypesRequestExecutorBuilderExtensions;
using OrdersTypes = orders::Microsoft.Extensions.DependencyInjection.ApiTypesRequestExecutorBuilderExtensions;
using PaymentsTypes = payments::Microsoft.Extensions.DependencyInjection.ApiTypesRequestExecutorBuilderExtensions;

namespace Gateway.Tests;

/// <summary>
/// O schema versionado de cada subgraph e os .far do gateway precisam estar em dia com o código: se um
/// schema mudar sem rodar o scripts/compose-gateway.sh, estes testes falham.
/// </summary>
[TestFixture]
public class SchemaTests
{
    private static string Root([System.Runtime.CompilerServices.CallerFilePath] string path = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, "..", "..", "..", ".."));

    private static string Archive(string env) => Path.Combine(Root(), $"src/backend/Gateway/Gateway.Web.Api/gateway.{env}.far");

    private static string Normalize(string schema) => schema.Replace("\r\n", "\n").Trim();

    // Subgraph: o nome no gateway, o schema versionado e como montar o schema do código (sem banco).
    public static IEnumerable<TestCaseData> Subgraphs()
    {
        yield return new TestCaseData(catalog::Program.SubgraphName, "src/backend/Catalog/Catalog.Web.Api/schema.graphqls",
            (Func<IRequestExecutorBuilder, IRequestExecutorBuilder>)(b => CatalogTypes.AddApiTypes(b)),
            (Action<IServiceCollection>)(s => s.AddSingleton(Mock.Of<Catalog.Queries.Products.Queries.IProductQueries>())))
            .SetName("Catalog");
        yield return new TestCaseData(customers::Program.SubgraphName, "src/backend/Customers/Customers.Web.Api/schema.graphqls",
            (Func<IRequestExecutorBuilder, IRequestExecutorBuilder>)(b => CustomersTypes.AddApiTypes(b.AddAuthorization())),
            (Action<IServiceCollection>)(s => s.AddSingleton(Mock.Of<Shared.Code.IMemoryBus>())))
            .SetName("Customers");
        yield return new TestCaseData(orders::Program.SubgraphName, "src/backend/Store/Orders.Web.Api/schema.graphqls",
            (Func<IRequestExecutorBuilder, IRequestExecutorBuilder>)(b => OrdersTypes.AddApiTypes(b.AddAuthorization())),
            (Action<IServiceCollection>)(s => s.AddSingleton(Mock.Of<Shared.Code.IMemoryBus>())))
            .SetName("Orders");
        yield return new TestCaseData(payments::Program.SubgraphName, "src/backend/Payments/Payments.Web.Api/schema.graphqls",
            (Func<IRequestExecutorBuilder, IRequestExecutorBuilder>)(b => PaymentsTypes.AddApiTypes(b.ModifyOptions(o => o.StrictValidation = false))),
            (Action<IServiceCollection>)(s => s.AddSingleton(Mock.Of<Shared.Code.IMemoryBus>())))
            .SetName("Payments");
    }

    [TestCaseSource(nameof(Subgraphs))]
    public async Task Schema_versionado_e_o_do_codigo(string name, string schemaFile,
        Func<IRequestExecutorBuilder, IRequestExecutorBuilder> addTypes, Action<IServiceCollection> addServices)
    {
        var services = new ServiceCollection();
        addServices(services);
        var executor = await addTypes(services.AddGraphQLServer(name)).BuildRequestExecutorAsync(name);

        Normalize(executor.Schema.ToString()).Should().Be(Normalize(await File.ReadAllTextAsync(Path.Combine(Root(), schemaFile))),
            $"o schema {name} mudou: rode scripts/compose-gateway.sh e versione os arquivos gerados");
    }

    [Test]
    public async Task Far_tem_os_schemas_versionados([Values("local", "compose")] string env,
        [Values("catalog:src/backend/Catalog/Catalog.Web.Api/schema.graphqls",
            "customers:src/backend/Customers/Customers.Web.Api/schema.graphqls",
            "orders:src/backend/Store/Orders.Web.Api/schema.graphqls",
            "payments:src/backend/Payments/Payments.Web.Api/schema.graphqls")] string subgraph)
    {
        var (name, schemaFile) = (subgraph.Split(':')[0], subgraph.Split(':')[1]);
        using var archive = ZipFile.OpenRead(Archive(env));
        var entry = archive.GetEntry($"source-schemas/{name}/schema.graphqls");
        entry.Should().NotBeNull($"gateway.{env}.far não tem o subgraph {name}");
        using var reader = new StreamReader(entry!.Open());

        Normalize(await reader.ReadToEndAsync()).Should().Be(Normalize(await File.ReadAllTextAsync(Path.Combine(Root(), schemaFile))),
            $"gateway.{env}.far está desatualizado: rode scripts/compose-gateway.sh");
    }
}
