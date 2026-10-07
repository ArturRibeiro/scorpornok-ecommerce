namespace Orders.Infrastructure.Seeds;

public static class OrderContextSeed
{
    public static async Task Seed(this OrderContext dbContext)
    {
        await dbContext.Database.EnsureCreatedAsync();
        await dbContext.SaveChangesAsync();
    }
}