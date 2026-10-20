namespace Payments.Infrastructure.Messaging;

/// <summary>
/// Retentativa dos consumidores para falhas passageiras (banco ou broker fora por alguns minutos).
/// A mensagem fica com o consumidor, sem confirmação, enquanto ele espera: se o processo cair, o
/// broker a devolve à fila. Esgotadas as tentativas, o MassTransit a move para a fila _error.
/// </summary>
public static class TransientRetry
{
    /// <summary>
    /// Intervalos dobrando a partir de 3 s, limitados a 2 min: 3, 6, 12, 24, 48, 96 e 120 s.
    /// Somam cerca de 5 minutos, bem abaixo do consumer_timeout de 30 minutos do RabbitMQ.
    /// </summary>
    public static readonly TimeSpan[] Intervals =
    [
        TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(12), TimeSpan.FromSeconds(24),
        TimeSpan.FromSeconds(48), TimeSpan.FromSeconds(96), TimeSpan.FromMinutes(2)
    ];

    /// <param name="intervals">Só os testes trocam os intervalos, para não esperar minutos.</param>
    public static void UseTransientRetry(this IConsumePipeConfigurator configurator, TimeSpan[] intervals = null)
        => configurator.UseMessageRetry(r =>
        {
            // Mensagem malformada não muda com o tempo: vai direto para a fila de erro.
            r.Ignore<ArgumentException>();
            r.Intervals(intervals ?? Intervals);
        });
}
