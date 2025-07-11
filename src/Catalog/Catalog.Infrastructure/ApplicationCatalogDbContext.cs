using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Catalog.Domain.Products;
using Catalog.Infrastructure.EntityConfigurations;
using Catalog.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Catalog.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class ApplicationCatalogDbContext : DbContext, IApplicationCatalogDbContext
    {
        #region Properties

        public DbSet<Product> Products { get; set; }

        #endregion

        public ApplicationCatalogDbContext(DbContextOptions<ApplicationCatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<T> DbSet<T>() where T : class => Set<T>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfigurations());

            ConvertModelBuilderCase(modelBuilder);
        }

        private static void ConvertModelBuilderCase(ModelBuilder modelBuilder)
        {
            modelBuilder.Model.GetEntityTypes()
                .ToList()
                .ForEach(entity =>
                {
                    entity.SetTableName(ConvertCase(entity.GetTableName()));
                    ConvertIndexes(entity);
                    ConvertForeignKeys(entity);
                    ConvertKeys(entity);
                    ConvertColumns(entity);
                });
        }

        private static string ConvertCase(string value)
            => value.ToLower();

        private static void ConvertIndexes(IMutableEntityType entityType)
            => entityType.GetIndexes().ToList().ForEach(index => index.SetName(ConvertCase(index.GetName())));

        private static void ConvertForeignKeys(IMutableEntityType entityType)
            => entityType.GetForeignKeys().ToList()
                .ForEach(index => index.SetConstraintName(ConvertCase(index.GetConstraintName())));

        private static void ConvertKeys(IMutableEntityType entityType)
            => entityType.GetKeys().ToList().ForEach(index => index.SetName(ConvertCase(index.GetName())));

        private static void ConvertColumns(IMutableEntityType entityType)
            => entityType.GetProperties().ToList()
                .ForEach(index => index.SetColumnName(ConvertCase(index.GetColumnName())));


    }
}