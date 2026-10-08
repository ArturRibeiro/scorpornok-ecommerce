using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orders.Infrastructure.Email;

/// <summary>
/// Envia o e-mail do resultado do pagamento fora do consumidor: a cada PollSeconds busca os
/// pedidos Confirmed/Failed ainda sem e-mail, envia e marca um a um. Se o SMTP falhar, o
/// pedido continua sem marca e é tentado na próxima rodada; o status do pedido não espera.
/// </summary>
public class OrderEmailDispatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<SmtpOptions> options,
    ILogger<OrderEmailDispatcher> logger) : BackgroundService
{
    public const int BatchSize = 20;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PollSeconds));
        using var timer = new PeriodicTimer(interval);
        do
        {
            try
            {
                await DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // Ex.: banco fora do ar. A próxima rodada tenta de novo.
                logger.LogError(ex, "Falha ao buscar pedidos com e-mail de pagamento pendente.");
            }
        } while (await WaitNextAsync(timer, stoppingToken));
    }

    /// <summary>Uma rodada: devolve quantos e-mails foram enviados.</summary>
    public async Task<int> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<IOrderEmailSender>();

        var sent = 0;
        foreach (var order in await repository.GetAwaitingPaymentEmailAsync(BatchSize, cancellationToken))
        {
            if (!order.IsAwaitingPaymentEmail)
                continue;

            try
            {
                await sender.SendPaymentResultAsync(order, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                // Provavelmente o SMTP está fora: encerra a rodada e deixa os pedidos para a próxima.
                logger.LogWarning(ex, "Falha ao enviar o e-mail do pedido {OrderNumber}; nova tentativa na próxima rodada.",
                    order.OrderNumber);
                break;
            }

            // Marca logo depois de enviar: se o processo cair entre os dois, o cliente recebe outro e-mail.
            order.MarkPaymentEmailSent();
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            sent++;
        }

        return sent;
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
