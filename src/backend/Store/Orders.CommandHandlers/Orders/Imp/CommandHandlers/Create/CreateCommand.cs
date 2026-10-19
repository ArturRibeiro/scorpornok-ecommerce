namespace Orders.CommandHandlers.Orders.Imp.CommandHandlers.Create;

/// <param name="Payment">Pagamento do pedido (JSON <c>payment</c>); nulo registra o pedido sem solicitar cobrança.</param>
/// <param name="Customer">Comprador (JSON <c>customer</c>): obrigatório, mas não guardado no pedido.</param>
public record CreateCommand(
    Guid UserId,
    OrderAddressMessageResponse Address,
    IList<OrderItemMessageResponse> Items,
    PaymentMethod? Payment,
    string? Email = null,
    OrderBuyerMessage? Customer = null)
    : Message<CreateOrderResult>;

/// <summary>
/// Nome, telefone e CPF de quem compra, informados no checkout (pontuação aceita). Seguem para o
/// Customers na mensagem do pedido registrado, que cria o cadastro na primeira compra.
/// </summary>
public record OrderBuyerMessage(string? FullName, string? Phone, string? Cpf);

/// <summary>
/// Como o pedido será pago. O cartão nunca chega ao Orders: o navegador o troca por um token no
/// POST /card-tokens do Payments, e só o token segue na solicitação de pagamento.
/// </summary>
/// <param name="CardToken">Token do cartão gerado pelo Payments (tok_...).</param>
/// <param name="Installments">Quantidade de parcelas (padrão: 1).</param>
public record PaymentMethod(string? CardToken, int Installments = 1);
