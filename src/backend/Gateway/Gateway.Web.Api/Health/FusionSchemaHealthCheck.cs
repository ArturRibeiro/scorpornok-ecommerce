using HotChocolate.Execution;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway.Web.Api.Health;

/// <summary>
/// Pronto quando o schema composto (o <c>.far</c>) está carregado. Não consulta os subgraphs: o gateway
/// atende as outras APIs mesmo com uma delas fora.
/// </summary>
public sealed class FusionSchemaHealthCheck(IRequestExecutorProvider executors) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await executors.GetExecutorAsync(cancellationToken: cancellationToken);
            return HealthCheckResult.Healthy("Schema composto carregado.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("Schema composto não carregado.", exception);
        }
    }
}
