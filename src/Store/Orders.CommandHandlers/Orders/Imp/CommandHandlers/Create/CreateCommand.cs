namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;

public record CreateCommand(
    Guid UserId,
    OrderAddressMessageResponse Address,
    IList<OrderItemMessageResponse> Items,
    CreditCardPaymentCommand Card)
    : Message;

/// <summary>
/// Command que representa os dados necessários para processar um pagamento via cartão de crédito.
/// </summary>
/// <param name="OrderId">Identificador único da ordem de compra associada ao pagamento.</param>
/// <param name="CardHolderName">Nome do titular do cartão, conforme impresso no cartão.</param>
/// <param name="CardNumber">Número do cartão de crédito (em produção, deve ser tokenizado ou criptografado).</param>
/// <param name="ExpirationMonth">Mês de expiração do cartão (formato MM).</param>
/// <param name="ExpirationYear">Ano de expiração do cartão (formato AAAA).</param>
/// <param name="Cvv">Código de segurança do cartão (CVV/CVC).</param>
/// <param name="Amount">Valor total a ser cobrado no cartão.</param>
/// <param name="Installments">Quantidade de parcelas para o pagamento (padrão: 1).</param>
public record CreditCardPaymentCommand(
    Guid OrderId,
    string CardHolderName,
    string CardNumber,
    string ExpirationMonth,
    string ExpirationYear,
    string Cvv,
    decimal Amount,
    int Installments = 1
);