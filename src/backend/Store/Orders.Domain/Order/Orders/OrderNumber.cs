namespace Orders.Domain.Order.Orders;

/// <summary>
/// Número do pedido: "A" + 12 dígitos aleatórios do gerador criptográfico (ex.: A483920175536).
/// É o identificador externo do pedido (tela, e-mail, hub e chave de idempotência do Payments),
/// então não pode se repetir; o índice único no banco recusa uma colisão.
/// </summary>
public static class OrderNumber
{
    // Dois blocos de 6 dígitos (o RandomNumberGenerator só sorteia int): o primeiro sem zero à
    // esquerda, então sempre são 12 dígitos, uniformes entre 100000000000 e 999999999999.
    public static string New()
        => $"A{RandomNumberGenerator.GetInt32(100_000, 1_000_000)}{RandomNumberGenerator.GetInt32(0, 1_000_000):D6}";
}
