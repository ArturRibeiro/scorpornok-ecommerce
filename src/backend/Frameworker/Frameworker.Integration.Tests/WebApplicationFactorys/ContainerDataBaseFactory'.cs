using System;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace Frameworker.Integration.Tests.WebApplicationFactorys
{
    public class ContainerDataBaseFactory
    {
        public static TestcontainerDatabase Create(DataBaseType dataBaseType)
        {
            return dataBaseType switch
            {
                DataBaseType.Postgres => CreateContainerPostgreSql(),
                DataBaseType.Oracle => CreateContainerOracle(),
                DataBaseType.Sqlite => CreateContainerSqlite(),
                _ => throw new ArgumentOutOfRangeException(nameof(dataBaseType), dataBaseType, null)
            };
        }

        private static TestcontainerDatabase CreateContainerPostgreSql() =>
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration()
                {
                    Database = "xxx.PAINEL.DOCKER",
                    Username = "xxx",
                    Password = "xxx",
                    Port = 5432
                }).Build();

        private static TestcontainerDatabase CreateContainerOracle() =>
            new TestcontainersBuilder<OracleTestcontainer>()
                .WithDatabase(new OracleTestcontainerConfiguration()
                {
                    Database = "xxx.PAINEL.DOCKER",
                    Username = "xxx",
                    Password = "xxx"
                }).Build();
        
        private static TestcontainerDatabase CreateContainerSqlite() =>
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new SqliteTestcontainerConfiguration()
                {
                    Database = "xxx.PAINEL.DOCKER",
                    Username = "xxx",
                    Password = "xxx",
                    Port = 5432
                }).Build();
    }

    public class SqliteTestcontainerConfiguration : TestcontainerDatabaseConfiguration
    {
        public SqliteTestcontainerConfiguration()
            : base(image: "", defaultPort: 0, port: 0)
        {
        }
    }
}