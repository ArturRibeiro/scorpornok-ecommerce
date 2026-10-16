namespace Shared.Code.IntegrationEvents;

// Mensagens trocadas entre Orders e Payments pelo RabbitMQ. O MassTransit roteia
// pelo nome completo do tipo, então os dois lados usam estes mesmos records.

/// <summary>Orders → Payments: cobrar o pedido.</summary>
/// <param name="Amount">Total calculado pelo pedido, não o valor enviado pelo cliente.</param>
public record PaymentRequested(
    int OrderId,
    string OrderNumber,
    Guid CustomerId,
    decimal Amount,
    int Installments,
    CardData Card);

/// <summary>Dados do cartão. Trafegam na fila, mas o Payments não os persiste.</summary>
public record CardData(
    string CardHolderName,
    string CardNumber,
    string ExpirationMonth,
    string ExpirationYear,
    string Cvv);

/// <summary>Payments → Orders: pagamento aprovado.</summary>
public record PaymentApproved(string OrderNumber, Guid PaymentId);

/// <summary>Payments → Orders: pagamento recusado.</summary>
public record PaymentRejected(string OrderNumber, string Reason);

/// <summary>
/// Orders → Payments: qual é a situação do pagamento do pedido? Só consulta, nunca cobra.
/// Enviada quando o pedido passa do prazo sem resultado.
/// </summary>
/// <param name="Attempt">Tentativa de pagamento do pedido (1 até haver nova tentativa com outro cartão).</param>
public record PaymentStatusRequested(string OrderNumber, int Attempt = 1);

/// <summary>Payments → Orders: o pagamento do pedido ainda está sendo cobrado.</summary>
public record PaymentStatusPending(string OrderNumber);

/// <summary>
/// Payments → Orders: não houve pagamento para o pedido. A tentativa foi anulada e uma solicitação
/// que chegue depois não é cobrada.
/// </summary>
public record PaymentNotProcessed(string OrderNumber);
