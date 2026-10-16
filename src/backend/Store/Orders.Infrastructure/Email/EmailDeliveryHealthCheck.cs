using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.Infrastructure.Email;

/// <summary>
/// /health/email, fora do ready (o pedido não depende do e-mail):
/// - Unhealthy (503): a última rodada parou com o servidor de e-mail fora;
/// - Degraded (200): houve pedidos em falha definitiva nas últimas 24 h (a quantidade vai em data);
/// - Healthy nos demais casos.
/// </summary>
public class EmailDeliveryHealthCheck(EmailDeliveryState state, IOrderRepository repository) : IHealthCheck
{
    public const string Name = "email-delivery";
    public static readonly TimeSpan FailureWindow = TimeSpan.FromHours(24);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (state.ServerUnavailable)
            return HealthCheckResult.Unhealthy($"Servidor de e-mail indisponível: {state.LastServerError}");

        // Mesmo relógio do EmailFailedAt (DateTime.Now).
        var failed = await repository.CountEmailFailedSinceAsync(DateTime.Now - FailureWindow, cancellationToken);
        return failed == 0
            ? HealthCheckResult.Healthy("E-mails sendo entregues.")
            : HealthCheckResult.Degraded($"{failed} pedido(s) com e-mail em falha definitiva nas últimas 24 horas.",
                data: new Dictionary<string, object> { ["failedEmails"] = failed });
    }
}
