namespace Frameworker.Observability;

// Consultas ao banco fora de uma requisição, de uma mensagem ou de uma Activity aberta pelo próprio serviço (o
// polling do e-mail, dos pedidos pendentes e do outbox, o EnsureCreated e a seed) não vão para o trace: a cada 5 s
// virariam um trace de uma linha. O exportador ignora a Activity sem a marca Recorded.
public sealed class DropRootDatabaseSpansProcessor : BaseProcessor<Activity>
{
    public const string NpgsqlSource = "Npgsql";

    public override void OnStart(Activity activity)
    {
        if (activity.Source.Name == NpgsqlSource && activity.ParentSpanId == default)
            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
    }
}
