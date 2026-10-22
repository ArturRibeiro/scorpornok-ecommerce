namespace Orders.Tests.Infrastructure;

// Registra os spans da fonte Scorponok.Orders (as rodadas dos serviços em segundo plano) enquanto existir.
public sealed class OrdersActivityRecorder : IDisposable
{
    private readonly ActivityListener _listener;

    public List<Activity> Started { get; } = [];

    public OrdersActivityRecorder()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == OrdersTelemetry.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = Started.Add
        };
        ActivitySource.AddActivityListener(_listener);
    }

    public void Dispose() => _listener.Dispose();
}
