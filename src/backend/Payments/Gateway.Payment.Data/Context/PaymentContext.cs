namespace Gateway.Payment.Data.Context;

public class PaymentContext(DbContextOptions<PaymentContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Domain.Payment> Payments { get; set; }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken) > 0;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payment");
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
    }
}
