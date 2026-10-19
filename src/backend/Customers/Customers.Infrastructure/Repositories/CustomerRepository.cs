namespace Customers.Infrastructure.Repositories;

public class CustomerRepository(CustomerContext context) : ICustomerRepository
{
    public IUnitOfWork UnitOfWork => context;

    public Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public void Add(Customer customer) => context.Customers.Add(customer);

    public Task<bool> HasDeliveryAddressAsync(string orderNumber, CancellationToken cancellationToken = default)
        => context.DeliveryAddresses.AnyAsync(d => d.OrderNumber == orderNumber, cancellationToken);

    public void AddDeliveryAddress(DeliveryAddress deliveryAddress) => context.DeliveryAddresses.Add(deliveryAddress);
}
