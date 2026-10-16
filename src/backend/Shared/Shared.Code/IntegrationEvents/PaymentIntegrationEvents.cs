namespace Shared.Code.IntegrationEvents;

// Mensagens trocadas entre Orders e Payments pelo RabbitMQ. O MassTransit roteia
// pelo nome completo do tipo, então os dois lados usam estes mesmos records. Todas levam a
// tentativa de pagamento (Attempt): o Orders só aplica o resultado da tentativa atual.

/// <summary>Orders → Payments: cobrar o pedido.</summary>
/// <param name="Amount">Total calculado pelo pedido, não o valor enviado pelo cliente.</param>
/// <param name="CardToken">
/// Token gerado pelo POST /card-tokens do Payments: o cartão (número e CVV) nunca passa pelo
/// Orders nem pelas filas.
/// </param>
/// <param name="Attempt">
/// Tentativa de pagamento do pedido: 1 no registro, e mais uma a cada nova tentativa com outro cartão.
/// O padrão 1 vale para mensagens antigas, sem o campo.
/// </param>
public record PaymentRequested(
    int OrderId,
    string OrderNumber,
    Guid CustomerId,
    decimal Amount,
    int Installments,
    string CardToken,
    int Attempt = 1);

/// <summary>Payments → Orders: pagamento aprovado.</summary>
public record PaymentApproved(string OrderNumber, Guid PaymentId, int Attempt = 1);

/// <summary>Payments → Orders: pagamento recusado.</summary>
public record PaymentRejected(string OrderNumber, string Reason, int Attempt = 1);

/// <summary>
/// Orders → Payments: qual é a situação do pagamento do pedido? Só consulta, nunca cobra.
/// Enviada quando o pedido passa do prazo sem resultado.
/// </summary>
/// <param name="Attempt">Tentativa atual do pagamento do pedido.</param>
public record PaymentStatusRequested(string OrderNumber, int Attempt = 1);

/// <summary>Payments → Orders: o pagamento do pedido ainda está sendo cobrado.</summary>
public record PaymentStatusPending(string OrderNumber, int Attempt = 1);

/// <summary>
/// Payments → Orders: não houve pagamento para o pedido. A tentativa foi anulada e uma solicitação
/// que chegue depois não é cobrada.
/// </summary>
public record PaymentNotProcessed(string OrderNumber, int Attempt = 1);
