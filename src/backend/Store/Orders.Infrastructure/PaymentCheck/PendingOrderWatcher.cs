namespace Orders.Infrastructure.PaymentCheck;

/// <summary>
/// Pedido pendente além do prazo: pergunta ao Payments (PaymentStatusRequested) o que aconteceu com
/// o pagamento, em vez de decidir sozinho. A consulta sai pelo outbox, gravada na mesma transação que
/// registra o StatusCheckedAt; a resposta volta pelos consumidores de sempre (aprovado, recusado) ou
/// pelos novos (não processado, em processamento). Enquanto não houver desfecho, o pedido é consultado
/// de novo a cada intervalo.
/// </summary>
public class PendingOrderWatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<PaymentCheckOptions> options,
    ILogger<PendingOrderWatcher> logger) : BackgroundService
{
    public const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(options.Value.Interval);
        do
        {
            try
            {
                await CheckDueOrdersAsync(DateTime.Now, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Ex.: banco fora do ar. A próxima rodada tenta de novo.
                logger.LogError(ex, "Falha ao verificar os pedidos pendentes.");
            }
        } while (await WaitNextAsync(timer, stoppingToken));
    }

    /// <summary>
    /// Uma rodada: devolve quantos pedidos foram consultados. O relógio é o de Order.OrderDate e da
    /// nova tentativa (DateTime.Now), para o prazo ser medido na mesma base.
    /// </summary>
    public async Task<int> CheckDueOrdersAsync(DateTime now, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

        var due = (await repository.GetPaymentCheckDueAsync(now - settings.Timeout, now - settings.Interval, BatchSize,
                cancellationToken))
            .Where(order => order.IsPaymentCheckDue(now, settings.Timeout, settings.Interval))
            .ToList();
        if (due.Count == 0)
            return 0;

        // O publicador grava no outbox; as mensagens e o StatusCheckedAt vão no mesmo SaveChanges.
        foreach (var order in due)
        {
            await publisher.PublishAsync(new PaymentStatusRequested(order.OrderNumber, order.Payment.Attempt), cancellationToken);
            order.MarkPaymentStatusChecked(now);
            logger.LogInformation("Pedido {OrderNumber} pendente desde {StartedAt} (tentativa {Attempt}); consultando o Payments.",
                order.OrderNumber, order.AttemptStartedAt, order.Payment.Attempt);
        }
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        return due.Count;
    }

    private static async Task<bool> WaitNextAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
