namespace Customers.Infrastructure.Messaging;

/// <summary>
/// Retentativa do consumidor para falhas passageiras (banco fora por alguns minutos, ou gravação
/// concorrente que esbarrou num índice único). Mesma política do Orders e do Payments: a mensagem
/// fica com o consumidor, sem confirmação, enquanto ele espera; esgotadas as tentativas, vai para a
/// fila _error.
/// </summary>
public static class TransientRetry
{
    /// <summary>Intervalos dobrando a partir de 3 s, limitados a 2 min (cerca de 5 minutos no total).</summary>
    public static readonly TimeSpan[] Intervals =
    [
        TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(12), TimeSpan.FromSeconds(24),
        TimeSpan.FromSeconds(48), TimeSpan.FromSeconds(96), TimeSpan.FromMinutes(2)
    ];

    /// <param name="intervals">Só os testes trocam os intervalos, para não esperar minutos.</param>
    public static void UseTransientRetry(this IConsumePipeConfigurator configurator, TimeSpan[]? intervals = null)
        => configurator.UseMessageRetry(r =>
        {
            // Mensagem malformada não muda com o tempo: vai direto para a fila de erro.
            r.Ignore<ArgumentException>();
            r.Intervals(intervals ?? Intervals);
        });
}
