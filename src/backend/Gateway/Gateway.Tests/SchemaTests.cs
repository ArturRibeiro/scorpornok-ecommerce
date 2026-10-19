extern alias catalog;
using System.IO.Compression;
using Catalog.Queries.Products.Queries;
using HotChocolate.Execution;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using CatalogProgram = catalog::Program;
using CatalogTypes = catalog::Microsoft.Extensions.DependencyInjection.ApiTypesRequestExecutorBuilderExtensions;

namespace Gateway.Tests;

/// <summary>
/// O schema versionado do Catalog e os .far do gateway precisam estar em dia com o código do Catalog: se o
/// schema mudar sem rodar o scripts/compose-gateway.sh, estes testes falham.
/// </summary>
[TestFixture]
public class SchemaTests
{
    private static string Root([System.Runtime.CompilerServices.CallerFilePath] string path = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, "..", "..", "..", ".."));

    private static string CatalogSchemaFile => Path.Combine(Root(), "src/backend/Catalog/Catalog.Web.Api/schema.graphqls");

    private static string Archive(string env) => Path.Combine(Root(), $"src/backend/Gateway/Gateway.Web.Api/gateway.{env}.far");

    private static string Normalize(string schema) => schema.Replace("\r\n", "\n").Trim();

    [Test]
    public async Task Schema_versionado_do_Catalog_e_o_do_codigo()
    {
        // Os mesmos tipos do Catalog.Web.Api (AddApiTypes, gerado pelo Hot Chocolate), sem banco.
        var server = new ServiceCollection()
            .AddSingleton(Mock.Of<IProductQueries>())
            .AddGraphQLServer(CatalogProgram.SubgraphName);
        var executor = await CatalogTypes.AddApiTypes(server).BuildRequestExecutorAsync(CatalogProgram.SubgraphName);

        Normalize(executor.Schema.ToString()).Should().Be(Normalize(await File.ReadAllTextAsync(CatalogSchemaFile)),
            "o schema do Catalog mudou: rode scripts/compose-gateway.sh e versione os arquivos gerados");
    }

    [TestCase("local")]
    [TestCase("compose")]
    public async Task Far_tem_o_schema_versionado_do_Catalog(string env)
    {
        using var archive = ZipFile.OpenRead(Archive(env));
        var entry = archive.GetEntry("source-schemas/catalog/schema.graphqls");
        entry.Should().NotBeNull();
        using var reader = new StreamReader(entry!.Open());

        Normalize(await reader.ReadToEndAsync()).Should().Be(Normalize(await File.ReadAllTextAsync(CatalogSchemaFile)),
            $"gateway.{env}.far está desatualizado: rode scripts/compose-gateway.sh");
    }
}
