namespace Orders.Web.Api.WebApplicationExtensions;

public static class ApplicationCatalogDbContextExtensions
{
    public static async Task Seed(this OrderContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.SaveChangesAsync();
    }
}