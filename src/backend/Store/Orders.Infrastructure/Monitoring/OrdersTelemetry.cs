namespace Orders.Infrastructure.Monitoring;

/// <summary>
/// Spans dos serviços em segundo plano (add-service-telemetry). Uma rodada só abre span quando encontra
/// trabalho: a consulta de polling fica fora do trace, e o envio, a gravação e a publicação pelo outbox ficam
/// dentro dele, levando o contexto para o Payments.
/// </summary>
public static class OrdersTelemetry
{
    public const string SourceName = "Scorponok.Orders";

    public static readonly ActivitySource Source = new(SourceName);
}
