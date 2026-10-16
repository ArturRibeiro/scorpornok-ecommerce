using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Orders.Infrastructure.PaymentCheck;

namespace Orders.Infrastructure.Monitoring;

/// <summary>
/// /health/ops: acusa pedidos pendentes além do prazo de verificação somado à tolerância
/// (PaymentCheck:TimeoutMinutes + AlertAfterMinutes), ou seja, presos mesmo com as consultas ao Payments.
/// </summary>
public class StuckOrdersHealthCheck(IOrderRepository repository, IOptions<PaymentCheckOptions> options) : IHealthCheck
{
    public const string Name = "stuck-orders";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var stuckAfter = options.Value.StuckAfter;
        // Mesmo relógio de Order.OrderDate (DateTime.Now).
        var count = await repository.CountPendingCreatedBeforeAsync(DateTime.Now - stuckAfter, cancellationToken);
        return count == 0
            ? HealthCheckResult.Healthy("Nenhum pedido preso.")
            : HealthCheckResult.Unhealthy(
                $"{count} pedido(s) pendente(s) há mais de {stuckAfter.TotalMinutes:0} minutos.",
                data: new Dictionary<string, object> { ["stuckOrders"] = count });
    }
}
