namespace Payments.CommandHandlers.Payments;

/// <summary>Processa o pagamento de um pedido; vem da mensagem <see cref="PaymentRequested"/>.</summary>
public record ProcessPaymentCommand(
    int OrderId,
    string OrderNumber,
    decimal Amount,
    int Installments,
    string CardToken,
    int Attempt = 1) : Message;
