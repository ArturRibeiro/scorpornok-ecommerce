namespace Payments.CommandHandlers.Payments;

/// <summary>Consulta da situação do pagamento de um pedido; vem da mensagem <see cref="PaymentStatusRequested"/>.</summary>
public record CheckPaymentStatusCommand(string OrderNumber, int Attempt) : Message;
