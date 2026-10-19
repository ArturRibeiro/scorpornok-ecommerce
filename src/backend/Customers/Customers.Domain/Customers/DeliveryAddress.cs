namespace Customers.Domain.Customers;

/// <summary>
/// Endereço de entrega de um pedido, guardado no histórico do cliente. Não muda o cadastro; o
/// número do pedido é único, então a mesma mensagem recebida de novo não gera outro registro.
/// </summary>
public class DeliveryAddress : Entity<int>
{
    //EF
    protected DeliveryAddress() { }

    public DeliveryAddress(Guid customerId, string orderNumber, Address address, DateTime orderDate)
    {
        CustomerId = customerId;
        OrderNumber = orderNumber;
        Address = address;
        OrderDate = orderDate;
    }

    public Guid CustomerId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public DateTime OrderDate { get; private set; }
}
