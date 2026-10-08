namespace Orders.Domain.Order.Orders;

/// <summary>
/// Pedido
/// </summary>
public class Order : Entity<int>, IAggregateRoot
{
    private readonly List<OrderItem> _items = new List<OrderItem>();

    #region Constructor
    public Order() { }

    public Order(Guid customerId)
    {
        CustomerId = customerId;
        this.OrderNumber = $"A{this.GetHashCode().ToString()}";
    }
    #endregion

    #region Properties

    public IReadOnlyCollection<OrderItem> Items => new ReadOnlyCollection<OrderItem>(_items);
    public OrderAddress Address { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string OrderNumber { get; private set; }
    public DateTime OrderDate { get; private set; } = DateTime.Now;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal Total { get; private set; }
    

    #endregion

    public void AddProduct(IList<OrderItem> items)
    {
        _items.AddRange(items);
        Total = _items.Sum(i => i.UnitPrice * i.Quantity);
    }
    
    public void ChangeStatus(OrderStatus failed) => Status = failed;

    /// <summary>Pagamento aprovado: confirma o pedido. Só age sobre pedido pendente.</summary>
    /// <returns>Se o pedido mudou; resultado repetido ou atrasado não altera nada.</returns>
    public bool ConfirmPayment(Guid paymentId)
    {
        if (!IsPending) return false;
        PaymentId = paymentId;
        Status = OrderStatus.Confirmed;
        return true;
    }

    /// <summary>Pagamento recusado: marca a falha. Só age sobre pedido pendente.</summary>
    /// <returns>Se o pedido mudou.</returns>
    public bool FailPayment()
    {
        if (!IsPending) return false;
        Status = OrderStatus.Failed;
        return true;
    }

    private bool IsPending => Status.Code == OrderStatus.Pending.Code;
    public void AddAddress(OrderAddress address) => this.Address = address;
    public void RemoveItem(int productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null) _items.Remove(item);
    }

    public decimal GetTotal() => _items.Sum(i => i.UnitPrice * i.Quantity);
    
    public override bool IsValid()
    {
        var orderValidation = new OrderValidation().Validate(this);
        if (!orderValidation.IsValid)
            this.SetValidation(orderValidation);
        return orderValidation.IsValid;
    }


    
}