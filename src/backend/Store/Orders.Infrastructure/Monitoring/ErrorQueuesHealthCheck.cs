namespace Orders.Infrastructure.Monitoring;

/// <summary>Filas de erro (<c>&lt;fila&gt;_error</c>) das filas que esta API consome.</summary>
public record ErrorQueues(IReadOnlyList<string> Names);

/// <summary>
/// /health/ops: acusa mensagens paradas nas filas de erro, que ninguém lê. Consulta a API de
/// gerenciamento do RabbitMQ (RabbitMq:ManagementUrl); fila inexistente conta como vazia, e a API
/// de gerenciamento fora do ar deixa o check Degraded (não sabemos, mas não é falha do fluxo).
/// </summary>
public class ErrorQueuesHealthCheck(HttpClient httpClient, ErrorQueues queues) : IHealthCheck
{
    public const string Name = "error-queues";
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    private sealed record QueueResponse(int Messages);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (httpClient.BaseAddress is null)
            return HealthCheckResult.Degraded("RabbitMq:ManagementUrl não configurado.");

        var withMessages = new Dictionary<string, object>();
        try
        {
            foreach (var queue in queues.Names)
            {
                var messages = await CountAsync(queue, cancellationToken);
                if (messages > 0) withMessages[queue] = messages;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or System.Text.Json.JsonException
                                       || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            return HealthCheckResult.Degraded("API de gerenciamento do RabbitMQ indisponível.", ex);
        }

        return withMessages.Count == 0
            ? HealthCheckResult.Healthy("Filas de erro vazias.")
            : HealthCheckResult.Unhealthy(
                "Mensagens nas filas de erro: " + string.Join(", ", withMessages.Select(q => $"{q.Key}={q.Value}")),
                data: withMessages);
    }

    private async Task<int> CountAsync(string queue, CancellationToken cancellationToken)
    {
        // %2F é o vhost padrão "/".
        using var response = await httpClient.GetAsync($"api/queues/%2F/{Uri.EscapeDataString(queue)}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return 0; // a fila de erro só é criada na primeira mensagem que esgota as tentativas
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"RabbitMQ respondeu {(int)response.StatusCode} para {queue}.");
        var body = await response.Content.ReadFromJsonAsync<QueueResponse>(cancellationToken);
        return body?.Messages ?? 0;
    }
}

public static class ErrorQueuesHealthCheckExtensions
{
    /// <summary>
    /// Check "error-queues" (tag ops) sobre as filas de erro de <paramref name="consumedQueues"/>, com
    /// o endereço (RabbitMq:ManagementUrl) e as credenciais (RabbitMq:Username/Password) do broker.
    /// </summary>
    public static IServiceCollection AddErrorQueuesHealthCheck(this IServiceCollection services,
        IConfiguration configuration, params string[] consumedQueues)
    {
        var rabbitMq = configuration.GetSection("RabbitMq");
        var managementUrl = rabbitMq["ManagementUrl"];
        services.AddSingleton(new ErrorQueues(consumedQueues.Select(queue => $"{queue}_error").ToList()));
        services.AddHttpClient<ErrorQueuesHealthCheck>(client =>
        {
            if (!string.IsNullOrWhiteSpace(managementUrl))
                client.BaseAddress = new Uri(managementUrl.TrimEnd('/') + "/");
            client.Timeout = ErrorQueuesHealthCheck.Timeout;
            var credentials = $"{rabbitMq["Username"] ?? "guest"}:{rabbitMq["Password"] ?? "guest"}";
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credentials)));
        });
        services.AddHealthChecks().AddCheck<ErrorQueuesHealthCheck>(ErrorQueuesHealthCheck.Name, tags: ["ops"]);
        return services;
    }
}
