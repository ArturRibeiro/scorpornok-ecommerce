using System.Diagnostics;

namespace Shared.Code.Telemetry;

/// <summary>
/// Número do pedido no span corrente (atributo <c>order.number</c>), para achar a compra na telemetria pelo número
/// (add-service-telemetry). Só <see cref="Activity"/>: os handlers não dependem do OpenTelemetry, e sem
/// telemetria ligada não há span e nada acontece. O número não é dado sensível (vai no e-mail e na URL da loja).
/// </summary>
public static class OrderTelemetry
{
    public const string OrderNumberTag = "order.number";

    public static void Tag(string? orderNumber)
    {
        if (!string.IsNullOrEmpty(orderNumber))
            Activity.Current?.SetTag(OrderNumberTag, orderNumber);
    }
}
