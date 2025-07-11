namespace Catalog.Web.Api;

public class DockerPostgreSql
{
    private static PostgreSqlContainer _postgresContainer;

    public static DockerPostgreSql Create()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("Catalog")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        
        return new DockerPostgreSql();
    }

    public async Task<DockerPostgreSql> Up()
    {
        await _postgresContainer.StartAsync();
        return this;
    }

    public string GetConnectionString() => _postgresContainer.GetConnectionString();

    public async Task DisposeAsync() => await _postgresContainer.DisposeAsync();
}