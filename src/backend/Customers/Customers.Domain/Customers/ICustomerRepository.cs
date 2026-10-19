namespace Customers.Domain.Customers;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Customer customer);

    /// <summary>Se o endereço do pedido já está no histórico (entrega repetida da mensagem).</summary>
    Task<bool> HasDeliveryAddressAsync(string orderNumber, CancellationToken cancellationToken = default);
    void AddDeliveryAddress(DeliveryAddress deliveryAddress);
}
