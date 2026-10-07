using System.Linq;

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
    public PaymentMethod PaymentMethod { get; private set; }
    

    #endregion

    public void AddProduct(IList<OrderItem> items)
    {
        _items.AddRange(items);
        Total = _items.Sum(i => i.UnitPrice * i.Quantity);
    }
    
    public void AddPaymentMethodCreditCard(PaymentMethod paymentMethod) => this.PaymentMethod = paymentMethod;
    public void ChangeStatus(OrderStatus failed) => Status = failed;
    public void AddAddress(OrderAddress address) => this.Address = address;
    public void RemoveItem(Guid productId)
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