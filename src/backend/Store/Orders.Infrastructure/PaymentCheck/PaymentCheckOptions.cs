namespace Orders.Infrastructure.PaymentCheck;

/// <summary>
/// Seção "PaymentCheck": quando o pedido pendente é considerado atrasado e o Payments passa a ser
/// consultado, de quanto em quanto tempo a consulta se repete e quando o pedido conta como preso.
/// </summary>
public class PaymentCheckOptions
{
    public const string Section = "PaymentCheck";

    /// <summary>Tempo de pedido pendente até a primeira consulta ao Payments.</summary>
    public int TimeoutMinutes { get; set; } = 15;

    /// <summary>Intervalo entre as rodadas do PendingOrderWatcher e entre consultas do mesmo pedido.</summary>
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>Tolerância, além do prazo, para o /health/ops acusar o pedido como preso.</summary>
    public int AlertAfterMinutes { get; set; } = 15;

    public TimeSpan Timeout => TimeSpan.FromMinutes(Math.Max(0, TimeoutMinutes));
    public TimeSpan Interval => TimeSpan.FromSeconds(Math.Max(1, IntervalSeconds));
    public TimeSpan StuckAfter => Timeout + TimeSpan.FromMinutes(Math.Max(0, AlertAfterMinutes));
}
