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
                    Database = "SAJ6.PAINEL.DOCKER",
                    Username = "saj",
                    Password = "agesune1",
                    Port = 5432
                }).Build();

        private static TestcontainerDatabase CreateContainerOracle() =>
            new TestcontainersBuilder<OracleTestcontainer>()
                .WithDatabase(new OracleTestcontainerConfiguration()
                {
                    Database = "SAJ6.PAINEL.DOCKER",
                    Username = "saj",
                    Password = "agesune1"
                }).Build();
        
        private static TestcontainerDatabase CreateContainerSqlite() =>
            new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new SqliteTestcontainerConfiguration()
                {
                    Database = "SAJ6.PAINEL.DOCKER",
                    Username = "saj",
                    Password = "agesune1",
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