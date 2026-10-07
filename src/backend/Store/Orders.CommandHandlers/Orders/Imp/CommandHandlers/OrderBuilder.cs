namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers;

public class OrderBuilder
{
    private readonly Order _order;
    private OrderAddress _address;
    private List<OrderItem> _items = new();
    private PaymentMethod _creditCardPayment;

    private OrderBuilder(Order order) => _order = order;

    public static OrderBuilder Create(Guid customerId)
    {
        var order = new Order(customerId);
        return new OrderBuilder(order);
    }

    public OrderBuilder AddAddress(string street, string city, string state, string country, string zipCode)
    {
        _address = OrderAddress.Factory.Create(street, city, state, country, zipCode);
        return this;
    }
    
    public OrderBuilder AddProduct(IList<OrderItemMessageResponse> items, Action<OrderItemMessageResponse, OrderBuilder> func)
    {
        items.ToList().ForEach(item => func(item, this));
        return this;
    }
    
    public OrderBuilder AddPaymentMethod(CreditCardPaymentCommand creditCardPaymentCommand, Func<CreditCardPaymentCommand, PaymentRequest> action)
    {
        action(creditCardPaymentCommand);
        return this;
    }

    public void CreateItem(Guid productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units)
    {
        _items.Add(OrderItem.Create(productId, productName, unitPrice, discount, pictureUrl, units));
    }
    
    public Order Build()
    {
        _order.AddAddress(_address);
        _order.AddProduct(_items);
        _order.AddPaymentMethodCreditCard(_creditCardPayment);
        return _order;
    }



}