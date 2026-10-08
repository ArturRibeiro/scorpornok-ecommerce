namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Payments;

/// <summary>Pagamento aprovado pelo Payments (mensagem PaymentApproved).</summary>
public record ConfirmOrderPaymentCommand(string OrderNumber, Guid PaymentId) : Message;

/// <summary>Pagamento recusado pelo Payments (mensagem PaymentRejected).</summary>
public record FailOrderPaymentCommand(string OrderNumber, string Reason) : Message;

/// <summary>Status atual do pedido (<c>OrderStatus.Name</c>), ou nulo se o número não existe.</summary>
public record GetOrderStatusQuery(string OrderNumber) : Message<string?>;
