namespace Orders.Infrastructure.Email;

/// <summary>Resultado de uma rodada: e-mails enviados e se a rodada parou com o servidor de e-mail fora.</summary>
public record EmailDispatchResult(int Sent, bool ServerUnavailable);

/// <summary>
/// Envia o e-mail do resultado do pagamento fora do consumidor: a cada rodada busca os pedidos com
/// desfecho ainda sem e-mail, envia pela mesma conexão e marca um a um. Cada falha é tratada pelo seu tipo
/// (<see cref="EmailDeliveryFailure"/>):
/// - destinatário: falha definitiva do pedido, e a rodada segue;
/// - mensagem: conta tentativa e agenda a próxima com intervalo crescente (falha definitiva no limite);
/// - servidor (também ao abrir a conexão): encerra a rodada sem tocar nos pedidos, e a espera entre rodadas
///   dobra até o teto.
/// O status do pedido nunca espera o e-mail.
/// </summary>
public class OrderEmailDispatcher(
    IServiceScopeFactory scopeFactory,
    IOptions<SmtpOptions> options,
    EmailDeliveryState state,
    ILogger<OrderEmailDispatcher> logger) : BackgroundService
{
    public const int BatchSize = 20;

    private TimeSpan _wait = TimeSpan.FromSeconds(options.Value.PollSeconds);

    /// <summary>Espera até a próxima rodada: PollSeconds, ou maior enquanto o servidor de e-mail estiver fora.</summary>
    public TimeSpan NextWait => _wait;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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
        } while (await WaitNextAsync(stoppingToken));
    }

    /// <summary>Uma rodada; também ajusta a espera até a próxima e o estado lido pelo /health/email.</summary>
    public async Task<EmailDispatchResult> DispatchPendingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<IOrderEmailSender>();
        var smtp = options.Value;
        // Mesmo relógio de Order.OrderDate e do EmailSentAt (DateTime.Now).
        var now = DateTime.Now;

        var awaiting = (await repository.GetAwaitingPaymentEmailAsync(now, BatchSize, cancellationToken))
            .Where(order => order.IsAwaitingPaymentEmailAt(now))
            .ToList();
        if (awaiting.Count == 0)
            return new EmailDispatchResult(0, ServerUnavailable: false);

        using var activity = OrdersTelemetry.Source.StartActivity("Envio dos e-mails de pagamento");
        activity?.SetTag("orders.count", awaiting.Count);

        // Uma conexão com o servidor para a rodada inteira, aberta só agora que há e-mail a enviar. Não conseguir
        // abrir (servidor fora, TLS, autenticação) vale para todos os pedidos, como uma falha de servidor no envio.
        IOrderEmailSession session;
        try
        {
            session = await sender.OpenSessionAsync(cancellationToken);
        }
        catch (EmailDeliveryException ex)
        {
            ServerUnavailable(ex);
            return new EmailDispatchResult(0, ServerUnavailable: true);
        }
        await using var _ = session;

        var sent = 0;
        foreach (var order in awaiting)
        {
            // Um span por pedido, com o número: a rodada inteira aparece ao procurar qualquer um deles.
            using var orderActivity = OrdersTelemetry.Source.StartActivity("Envio do e-mail de pagamento");
            OrderTelemetry.Tag(order.OrderNumber);
            try
            {
                await session.SendPaymentResultAsync(order, cancellationToken);
            }
            catch (EmailDeliveryException ex) when (ex.Kind == EmailDeliveryFailure.Server)
            {
                // Vale para todos os pedidos: não conta tentativa de ninguém e espera mais até a próxima rodada.
                ServerUnavailable(ex);
                return new EmailDispatchResult(sent, ServerUnavailable: true);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                var kind = ex is EmailDeliveryException delivery ? delivery.Kind : EmailDeliveryFailure.Message;
                RegisterFailure(order, kind, ex.Message, now, smtp);
                await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
                continue;
            }

            // Marca logo depois de enviar: se o processo cair entre os dois, o cliente recebe outro e-mail.
            order.MarkPaymentEmailSent();
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            sent++;
            ServerAvailable();
        }

        return new EmailDispatchResult(sent, ServerUnavailable: false);
    }

    private void RegisterFailure(Order order, EmailDeliveryFailure kind, string reason, DateTime now, SmtpOptions smtp)
    {
        var attempt = order.Payment.EmailAttempts + 1;
        if (kind == EmailDeliveryFailure.Recipient || attempt >= smtp.MaxEmailAttempts)
        {
            order.MarkPaymentEmailFailed(now, reason);
            logger.LogError("E-mail do pedido {OrderNumber} em falha definitiva após {Attempts} tentativa(s): {Reason}",
                order.OrderNumber, attempt, reason);
            return;
        }

        var delay = smtp.RetryDelay(attempt);
        order.RegisterPaymentEmailRetry(now, delay, reason);
        logger.LogWarning("Falha temporária no e-mail do pedido {OrderNumber} (tentativa {Attempt}); nova tentativa em {Delay}: {Reason}",
            order.OrderNumber, attempt, delay, reason);
    }

    private void ServerUnavailable(EmailDeliveryException ex)
    {
        var smtp = options.Value;
        // Log só na primeira rodada com o servidor fora, para não inundar o log enquanto ele não volta.
        if (!state.ServerUnavailable)
            logger.LogWarning(ex, "Servidor de e-mail indisponível; os envios esperam ele voltar: {Reason}", ex.Message);
        state.MarkServerUnavailable(ex.Message);
        _wait = TimeSpan.FromSeconds(Math.Min(_wait.TotalSeconds * 2, smtp.RetryMaxSeconds));
    }

    private void ServerAvailable()
    {
        if (state.ServerUnavailable)
            logger.LogInformation("Servidor de e-mail de volta; envios retomados.");
        state.MarkServerAvailable();
        _wait = TimeSpan.FromSeconds(options.Value.PollSeconds);
    }

    private async Task<bool> WaitNextAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(_wait, stoppingToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
