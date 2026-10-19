namespace Customers.Infrastructure;

[ExcludeFromCodeCoverage]
public class CustomerContext(DbContextOptions<CustomerContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<DeliveryAddress> DeliveryAddresses => Set<DeliveryAddress>();

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        => await base.SaveChangesAsync(cancellationToken) > 0;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("customer");
        modelBuilder.ApplyConfiguration(new CustomerConfigurations());
        modelBuilder.ApplyConfiguration(new DeliveryAddressConfigurations());
    }
}
