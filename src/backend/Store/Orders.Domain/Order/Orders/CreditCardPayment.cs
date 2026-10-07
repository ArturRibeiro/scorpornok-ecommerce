using Orders.Domain.Order.Orders;

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
public class PaymentMethod(
    int OrderId,
    string CardHolderName,
    string CardNumber,
    string ExpirationMonth,
    string ExpirationYear,
    string Cvv,
    decimal Amount,
    int Installments = 1
) : Entity<int>
{
    public int OrderId { get; init; } = OrderId;
    public string CardHolderName { get; init; } = CardHolderName;
    public string CardNumber { get; init; } = CardNumber;
    public string ExpirationMonth { get; init; } = ExpirationMonth;
    public string ExpirationYear { get; init; } = ExpirationYear;
    public string Cvv { get; init; } = Cvv;
    public decimal Amount { get; init; } = Amount;
    public int Installments { get; init; } = Installments;
}