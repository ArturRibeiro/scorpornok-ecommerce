namespace Orders.Domain.Order.Orders;

/// <summary>
/// Pagamento do pedido (1:1): nasce vazio com o pedido e guarda o id do pagamento
/// aprovado e quando o e-mail com o resultado foi enviado. Só o <see cref="Order"/>
/// o altera, para que as regras de status continuem no agregado.
/// </summary>
public class OrderPayment
{
    /// <summary>Id do pagamento no Payments; nulo até o pagamento ser aprovado.</summary>
    public Guid? PaymentId { get; private set; }

    /// <summary>Quando o e-mail com o resultado do pagamento foi enviado; nulo enquanto não foi.</summary>
    public DateTime? EmailSentAt { get; private set; }

    /// <summary>Última consulta ao Payments sobre o pedido pendente; nula enquanto não houve.</summary>
    public DateTime? StatusCheckedAt { get; private set; }

    internal void Approve(Guid paymentId) => PaymentId = paymentId;

    /// <summary>Só a primeira marcação vale.</summary>
    internal void MarkEmailSent() => EmailSentAt ??= DateTime.Now;

    internal void MarkStatusChecked(DateTime at) => StatusCheckedAt = at;
}
