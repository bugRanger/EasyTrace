using EasyTrace.Activity;

namespace EasyTrace.Export.Batch;

public sealed class BatchExporter<T> : IDisposable
    where T : ITraceActivityExporter
{
    private readonly BatchExportWorker<T> _backgroundExporter;
    private bool _disposed;

    public BatchExporter(T exporter, BatchExportOptions options)
    {
        _backgroundExporter = new BatchExportWorker<T>(exporter, options.MaxQueueSize);
        _backgroundExporter.MaxExportBatchSize = options.MaxExportBatchSize;
        _backgroundExporter.ScheduledDelayMilliseconds = options.ScheduledDelayMilliseconds;
        _backgroundExporter.Start();
    }

    public void Append(scoped in TraceActivityRef activityRef)
    {
        if (!_backgroundExporter.CircularBuffer.Push(in activityRef, 50_000))
        {
            return;
        }

        _ = _backgroundExporter.TryExport();
    }

    ~BatchExporter()
    {
        Dispose(false);
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
            _backgroundExporter.Dispose();
        }

        _disposed = true;
    }
}