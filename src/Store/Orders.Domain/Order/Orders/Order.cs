namespace Orders.Domain.Order.Orders;

/// <summary>
/// Pedido
/// </summary>
public class Order : Entity<int>, IAggregateRoot
{
    private readonly List<OrderItem> _items = new List<OrderItem>();

    #region Constructor
    public Order() { }

    public Order(Guid customerId, Guid paymentId)
    {
        CustomerId = customerId;
        PaymentId = paymentId;
        this.OrderNumber = $"A{this.GetHashCode().ToString()}";
    }
    #endregion

    #region Properties

    public IReadOnlyCollection<OrderItem> Items => new ReadOnlyCollection<OrderItem>(_items);

    /// <summary>
    /// Endereço de entrega
    /// </summary>
    public OrderAddress Address { get; private set; }

    /// <summary>
    /// Cliente que realizou a compra
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Código do Pedido
    /// </summary>
    public string OrderNumber { get; private set; }

    public DateTime OrderDate { get; private set; } = DateTime.Now;

    public OrderStatus Status { get; private set; } = OrderStatus.Submitted;

    /// <summary>
    /// Códido da forma como foi paga o pedido
    /// </summary>
    public Guid PaymentId { get; private set; }

    #endregion

    public void AddProduct(OrderItem item) => _items.Add(item);
    public void AddProduct(IList<OrderItem> items) => _items.AddRange(items);
    public void AddAddress(OrderAddress address) => this.Address = address;
    
    public override bool IsValid()
    {
        var orderValidation = new OrderValidation().Validate(this);
        if (!orderValidation.IsValid)
            this.SetValidation(orderValidation);
        return orderValidation.IsValid;
    }
}