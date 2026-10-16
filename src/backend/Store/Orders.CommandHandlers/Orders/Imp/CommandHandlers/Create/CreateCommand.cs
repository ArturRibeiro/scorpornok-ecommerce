namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;

/// <param name="Payment">Pagamento do pedido (JSON <c>payment</c>); nulo registra o pedido sem solicitar cobrança.</param>
public record CreateCommand(
    Guid UserId,
    OrderAddressMessageResponse Address,
    IList<OrderItemMessageResponse> Items,
    PaymentMethod? Payment,
    string? Email = null)
    : Message<CreateOrderResult>;

/// <summary>
/// Como o pedido será pago. O cartão nunca chega ao Orders: o navegador o troca por um token no
/// POST /card-tokens do Payments, e só o token segue na solicitação de pagamento.
/// </summary>
/// <param name="CardToken">Token do cartão gerado pelo Payments (tok_...).</param>
/// <param name="Installments">Quantidade de parcelas (padrão: 1).</param>
public record PaymentMethod(string? CardToken, int Installments = 1);
