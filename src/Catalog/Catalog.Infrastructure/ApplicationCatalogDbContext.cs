using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Products;
using Catalog.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Shared.Code.Models;

namespace Catalog.Infrastructure
{
    public class ApplicationCatalogDbContext : DbContext, IUnitOfWork
    {
        #region Properties
        public DbSet<Product> Products { get; set; }
        #endregion

        public ApplicationCatalogDbContext(DbContextOptions<ApplicationCatalogDbContext> options) : base(options) { }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.SaveChangesAsync();

            return result > 0;
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfigurations());
        }
    }
}
