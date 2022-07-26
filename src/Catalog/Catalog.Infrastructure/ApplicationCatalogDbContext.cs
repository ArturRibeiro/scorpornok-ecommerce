using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Domain.Products;
using Catalog.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Shared.Code.Models;

namespace Catalog.Infrastructure
{
    [ExcludeFromCodeCoverage]
    public class ApplicationCatalogDbContext : DbContext, IUnitOfWork
    {
        #region Properties
        public DbSet<Product> Products { get; set; }
        #endregion

        public ApplicationCatalogDbContext(DbContextOptions<ApplicationCatalogDbContext> options) : base(options)
        {
            
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.SaveChangesAsync();

            return result > 0;
        }


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
            => entityType.GetForeignKeys().ToList().ForEach(index => index.SetConstraintName(ConvertCase(index.GetConstraintName())));

        private static void ConvertKeys(IMutableEntityType entityType)
            => entityType.GetKeys().ToList().ForEach(index => index.SetName(ConvertCase(index.GetName())));

        private static void ConvertColumns(IMutableEntityType entityType)
            => entityType.GetProperties().ToList().ForEach(index => index.SetColumnName(ConvertCase(index.GetColumnName())));
    }
}
