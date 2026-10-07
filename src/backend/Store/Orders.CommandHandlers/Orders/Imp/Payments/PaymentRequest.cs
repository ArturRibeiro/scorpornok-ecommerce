namespace Orders.CommandHandlers.Orders.Imp.Payments;

public record PaymentRequest(decimal Amount, string CardHolderName, string CardNumber, string ExpirationMonth,
    string ExpirationYear, string Cvv, int Installments);

/// <summary>
/// Resultado imediato ao enviar uma solicitação de pagamento ao gateway.
/// Indica apenas se a solicitação foi aceita para processamento, não o resultado final.
/// </summary>
/// <param name="IsRequestSuccessful">Indica se a solicitação foi recebida corretamente pelo gateway.</param>
/// <param name="RequestMessage">Identificador de rastreio da requisição no gateway.</param>
/// <param name="RequestId">Mensagem adicional ou código de resposta.</param>
public record PaymentResult(bool IsRequestSuccessful, string RequestMessage, int RequestId);