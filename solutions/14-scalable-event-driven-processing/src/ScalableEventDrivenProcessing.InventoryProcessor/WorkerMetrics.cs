using System.Diagnostics.Metrics;

namespace ScalableEventDrivenProcessing.InventoryProcessor;

internal static class WorkerMetrics
{
    private static int _ready;

    internal static bool IsReady => System.Threading.Volatile.Read(ref _ready) == 1;
    internal static void SetReady(bool ready) => System.Threading.Volatile.Write(ref _ready, ready ? 1 : 0);

    internal static readonly Meter Meter = new("ScalableEventDrivenProcessing.InventoryProcessor", "1.0.0");
    internal static readonly Counter<long> Processed = Meter.CreateCounter<long>("hpt.inventory.events.processed", "{event}");
    internal static readonly Counter<long> Failed = Meter.CreateCounter<long>("hpt.inventory.events.failed", "{event}");
    internal static readonly Counter<long> Committed = Meter.CreateCounter<long>("hpt.inventory.offsets.committed", "{offset}");
    internal static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("hpt.inventory.processing.duration", "ms");
}
