using Microsoft.EntityFrameworkCore;

namespace Catalog.Queries;

public interface IApplicationCatalogDbContext
{
    public DbSet<T> DbSet<T>()
        where T : class;
}