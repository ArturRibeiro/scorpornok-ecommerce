namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;

/// <summary>Pagamento aprovado pelo Payments (mensagem PaymentApproved).</summary>
public record ConfirmOrderPaymentCommand(string OrderNumber, Guid PaymentId, int Attempt = 1) : Message;

/// <summary>Pagamento recusado pelo Payments (mensagem PaymentRejected).</summary>
public record FailOrderPaymentCommand(string OrderNumber, string Reason, int Attempt = 1) : Message;

/// <summary>O Payments não tinha pagamento e anulou o pedido (mensagem PaymentNotProcessed).</summary>
public record MarkOrderPaymentNotProcessedCommand(string OrderNumber, int Attempt = 1) : Message;

/// <summary>
/// Status atual do pedido do cliente (<c>OrderStatus.Name</c>) e a tentativa atual de pagamento, ou
/// nulo se o número não existe ou é de outro cliente: quem pergunta não distingue os dois casos.
/// </summary>
public record GetOrderStatusQuery(string OrderNumber, Guid CustomerId) : Message<OrderPaymentState?>;

/// <summary>Status do pedido (<c>OrderStatus.Name</c>) e tentativa atual de pagamento.</summary>
public record OrderPaymentState(string Status, int Attempt);
