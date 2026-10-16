namespace Orders.Domain.Order.Orders;

/// <summary>
/// Pagamento do pedido (1:1): nasce vazio com o pedido e guarda o id do pagamento
/// aprovado e quando o e-mail com o resultado foi enviado. Só o <see cref="Order"/>
/// o altera, para que as regras de status continuem no agregado.
/// </summary>
public class OrderPayment
{
    /// <summary>
    /// Tentativa atual de pagamento: 1 no registro do pedido e mais uma a cada nova tentativa com
    /// outro cartão. Só o resultado desta tentativa muda o pedido.
    /// </summary>
    public int Attempt { get; private set; } = 1;

    /// <summary>Quando a tentativa atual foi solicitada; nulo na primeira (vale a data do pedido).</summary>
    public DateTime? AttemptRequestedAt { get; private set; }

    /// <summary>Id do pagamento no Payments; nulo até o pagamento ser aprovado.</summary>
    public Guid? PaymentId { get; private set; }

    /// <summary>Quando o e-mail com o resultado do pagamento foi enviado; nulo enquanto não foi.</summary>
    public DateTime? EmailSentAt { get; private set; }

    /// <summary>Última consulta ao Payments sobre o pedido pendente; nula enquanto não houve.</summary>
    public DateTime? StatusCheckedAt { get; private set; }

    /// <summary>Tentativas de envio do e-mail que falharam por problema da mensagem ou do destinatário.</summary>
    public int EmailAttempts { get; private set; }

    /// <summary>Quando o e-mail pode ser tentado de novo; nulo para tentar na próxima rodada.</summary>
    public DateTime? NextEmailAttemptAt { get; private set; }

    /// <summary>Quando o envio do e-mail foi encerrado como falha definitiva; nulo enquanto não foi.</summary>
    public DateTime? EmailFailedAt { get; private set; }

    /// <summary>Motivo da última falha de envio do e-mail.</summary>
    public string EmailFailureReason { get; private set; }

    internal void Approve(Guid paymentId) => PaymentId = paymentId;

    /// <summary>
    /// Começa a próxima tentativa: o prazo da consulta ao Payments e o envio do e-mail recomeçam,
    /// para que o resultado desta tentativa tenha a sua consulta e o seu e-mail.
    /// </summary>
    internal void StartNextAttempt(DateTime at)
    {
        Attempt++;
        AttemptRequestedAt = at;
        PaymentId = null;
        StatusCheckedAt = null;
        EmailSentAt = null;
        EmailAttempts = 0;
        NextEmailAttemptAt = null;
        EmailFailedAt = null;
        EmailFailureReason = null;
    }

    /// <summary>Só a primeira marcação vale.</summary>
    internal void MarkEmailSent() => EmailSentAt ??= DateTime.Now;

    internal void MarkStatusChecked(DateTime at) => StatusCheckedAt = at;

    internal void RegisterEmailRetry(DateTime nextAttemptAt, string reason)
    {
        EmailAttempts++;
        NextEmailAttemptAt = nextAttemptAt;
        EmailFailureReason = reason;
    }

    internal void MarkEmailFailed(DateTime at, string reason)
    {
        EmailAttempts++;
        EmailFailedAt ??= at;
        NextEmailAttemptAt = null;
        EmailFailureReason = reason;
    }
}
