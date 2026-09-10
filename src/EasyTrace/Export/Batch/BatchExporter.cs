using EasyTrace.Activity;

namespace EasyTrace.Export.Batch;

public sealed class BatchExporter<T>(T exporter, TraceActivityFactory factory, BatchExportOptions options)
    : IDisposable
    where T : ITraceActivityExporter
{
    private readonly BatchExportWorker<T> _backgroundExporter = new(exporter, factory, options);

    private bool _disposed;

    public void Handle(scoped in TraceActivityRef activityRef)
    {
        if (!_backgroundExporter.Push(in activityRef))
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