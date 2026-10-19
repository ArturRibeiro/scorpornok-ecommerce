namespace Customers.Infrastructure.Seeds;

public static class CustomerContextSeed
{
    // Sem migrations e sem dados iniciais: só cria o banco na partida.
    public static Task Seed(this CustomerContext dbContext) => dbContext.Database.EnsureCreatedAsync();
}
