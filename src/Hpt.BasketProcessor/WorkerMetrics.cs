using System.Diagnostics.Metrics;

namespace Hpt.BasketProcessor;

internal static class WorkerMetrics
{
    private static int _ready;

    internal static bool IsReady => System.Threading.Volatile.Read(ref _ready) == 1;
    internal static void SetReady(bool ready) => System.Threading.Volatile.Write(ref _ready, ready ? 1 : 0);

    internal static readonly Meter Meter = new("Hpt.BasketProcessor", "1.0.0");
    internal static readonly Counter<long> Processed = Meter.CreateCounter<long>("hpt.basket.events.processed", "{event}");
    internal static readonly Counter<long> Failed = Meter.CreateCounter<long>("hpt.basket.events.failed", "{event}");
    internal static readonly Counter<long> Committed = Meter.CreateCounter<long>("hpt.basket.offsets.committed", "{offset}");
    internal static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("hpt.basket.processing.duration", "ms");
    internal static readonly Histogram<int> PollBatchSize = Meter.CreateHistogram<int>("hpt.basket.poll.batch.size", "{message}");
}
