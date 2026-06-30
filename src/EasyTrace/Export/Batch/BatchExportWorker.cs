using EasyTrace.Activity;
using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Export.Batch;

public sealed class BatchExportWorker<T> : IDisposable
    where T : ITraceActivityExporter
{
    private readonly T _exporter;
    private readonly Thread _exporterThread;
    private readonly AutoResetEvent _exportTrigger = new(false);
    private bool _disposed;

    public BatchExportWorker(T exporter, uint limitQueueSize)
    {
        CircularBuffer = new CircularBuffer<TraceActivity>(limitQueueSize);
        _exporter = exporter;
        _exporterThread = new Thread(ExporterProc)
        {
            IsBackground = true, Name = $"Batch-Export-For-{exporter.GetType().Name}",
        };
    }

    /// <summary>
    /// Gets the circular buffer for storing telemetry objects.
    /// </summary>
    public CircularBuffer<TraceActivity> CircularBuffer { get; }

    /// <summary>
    /// Gets the maximum batch size for exports.
    /// </summary>
    public ulong MaxExportBatchSize { get; set; }

    /// <summary>
    /// Gets the delay between exports in milliseconds.
    /// </summary>
    public ulong ScheduledDelayMilliseconds { get; set; }

    ~BatchExportWorker()
    {
        Dispose(false);
    }

    public void Start()
    {
        _exporterThread.Start();
    }

    public bool TryExport()
    {
        if (CircularBuffer.Count < MaxExportBatchSize)
        {
            return false;
        }

        try
        {
            _exportTrigger.Set();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _exportTrigger.Set();
            _exportTrigger.Dispose();
        }

        _disposed = true;
    }

    private void ExporterProc()
    {
        var triggers = new WaitHandle[]
        {
            _exportTrigger
        };

        while (true)
        {
            if (CircularBuffer.Count < MaxExportBatchSize)
            {
                try
                {
                    WaitHandle.WaitAny(triggers, (int)ScheduledDelayMilliseconds);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }

            if (CircularBuffer.Count <= 0)
            {
                continue;
            }

            var activities = CircularBuffer.Next(MaxExportBatchSize);
            foreach (var activity in activities)
            {
                _exporter.Export(new TraceActivityRef(activity.Source, activity));
            }
        }
    }
}