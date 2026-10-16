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
        OrderNumber = Orders.OrderNumber.New();
    }
    #endregion

    #region Properties

    public IReadOnlyCollection<OrderItem> Items => new ReadOnlyCollection<OrderItem>(_items);
    public OrderAddress Address { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Email { get; private set; }
    public string OrderNumber { get; private set; }
    public DateTime OrderDate { get; private set; } = DateTime.Now;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public decimal Total { get; private set; }
    /// <summary>Pagamento do pedido (1:1), criado vazio junto com ele.</summary>
    public OrderPayment Payment { get; private set; } = new();
    

    #endregion

    public void AddProduct(IList<OrderItem> items)
    {
        _items.AddRange(items);
        Total = _items.Sum(i => i.UnitPrice * i.Quantity);
    }
    
    public void ChangeStatus(OrderStatus failed) => Status = failed;

    /// <summary>Tentativas de pagamento por pedido: a primeira e até duas novas, com outro cartão.</summary>
    public const int MaxPaymentAttempts = 3;

    /// <summary>Pagamento aprovado: confirma o pedido. Só age sobre pedido pendente e na tentativa atual.</summary>
    /// <returns>Se o pedido mudou; resultado repetido, atrasado ou de outra tentativa não altera nada.</returns>
    public bool ConfirmPayment(Guid paymentId, int attempt)
    {
        if (!IsPendingOn(attempt)) return false;
        Payment.Approve(paymentId);
        Status = OrderStatus.Confirmed;
        return true;
    }

    /// <summary>Pagamento recusado: marca a falha. Só age sobre pedido pendente e na tentativa atual.</summary>
    /// <returns>Se o pedido mudou.</returns>
    public bool FailPayment(int attempt)
    {
        if (!IsPendingOn(attempt)) return false;
        Status = OrderStatus.Failed;
        return true;
    }

    /// <summary>
    /// O Payments não tem pagamento para a tentativa e a anulou: nada foi cobrado. Só age sobre
    /// pedido pendente e na tentativa atual, como os outros desfechos.
    /// </summary>
    /// <returns>Se o pedido mudou.</returns>
    public bool MarkPaymentNotProcessed(int attempt)
    {
        if (!IsPendingOn(attempt)) return false;
        Status = OrderStatus.NotProcessed;
        return true;
    }

    /// <summary>
    /// O cliente pode pagar de novo, com outro cartão: só depois de uma recusa do cartão e enquanto
    /// houver tentativas. "Não processado" não entra: a tentativa foi anulada por um problema da loja,
    /// e o cliente é orientado a refazer a compra.
    /// </summary>
    public bool CanRetryPayment => Status.Code == OrderStatus.Failed.Code && Payment.Attempt < MaxPaymentAttempts;

    /// <summary>
    /// Nova tentativa de pagamento: o pedido volta a pendente na próxima tentativa, com o prazo e o
    /// e-mail recomeçando. Só age quando <see cref="CanRetryPayment"/>.
    /// </summary>
    /// <returns>Se o pedido mudou.</returns>
    public bool RetryPayment(DateTime now)
    {
        if (!CanRetryPayment) return false;
        Payment.StartNextAttempt(now);
        Status = OrderStatus.Pending;
        return true;
    }

    private bool IsPending => Status.Code == OrderStatus.Pending.Code;

    private bool IsPendingOn(int attempt) => IsPending && attempt == Payment.Attempt;

    /// <summary>Desde quando a tentativa atual espera resultado: o registro do pedido ou a nova tentativa.</summary>
    public DateTime AttemptStartedAt => Payment.AttemptRequestedAt ?? OrderDate;

    /// <summary>
    /// Pendente além do prazo e sem consulta ao Payments no último intervalo: é hora de perguntar
    /// ao Payments o que aconteceu com o pagamento.
    /// </summary>
    public bool IsPaymentCheckDue(DateTime now, TimeSpan timeout, TimeSpan interval)
        => IsPending
           && AttemptStartedAt <= now - timeout
           && (Payment.StatusCheckedAt is null || Payment.StatusCheckedAt <= now - interval);

    /// <summary>Registra a consulta ao Payments, para a próxima só sair depois do intervalo.</summary>
    public void MarkPaymentStatusChecked(DateTime now) => Payment.MarkStatusChecked(now);

    /// <summary>
    /// O pagamento já tem desfecho (confirmado, recusado ou não processado), o cliente ainda não recebeu
    /// o e-mail, o envio não está em falha definitiva e a próxima tentativa, se agendada, já venceu.
    /// </summary>
    public bool IsAwaitingPaymentEmail => IsAwaitingPaymentEmailAt(DateTime.Now);

    public bool IsAwaitingPaymentEmailAt(DateTime now)
        => Payment.EmailSentAt is null
           && Payment.EmailFailedAt is null
           && (Payment.NextEmailAttemptAt is null || Payment.NextEmailAttemptAt <= now)
           && (Status.Code == OrderStatus.Confirmed.Code || Status.Code == OrderStatus.Failed.Code
               || Status.Code == OrderStatus.NotProcessed.Code);

    /// <summary>Registra o envio do e-mail do resultado. Só a primeira marcação vale.</summary>
    public void MarkPaymentEmailSent() => Payment.MarkEmailSent();

    /// <summary>Falha temporária no envio do e-mail: conta a tentativa e agenda a próxima.</summary>
    public void RegisterPaymentEmailRetry(DateTime now, TimeSpan delay, string reason)
        => Payment.RegisterEmailRetry(now + delay, reason);

    /// <summary>Falha definitiva no envio do e-mail: o pedido deixa de ser tentado.</summary>
    public void MarkPaymentEmailFailed(DateTime now, string reason) => Payment.MarkEmailFailed(now, reason);
    public void AddAddress(OrderAddress address) => this.Address = address;
    public void AddEmail(string email) => Email = email?.Trim();
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