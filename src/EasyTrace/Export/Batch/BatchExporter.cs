using EasyTrace.Activity;

namespace EasyTrace.Export.Batch;

public sealed class BatchExporter(GroupExporter exporter, TraceActivityFactory factory, BatchExportOptions options)
    : IDisposable
{
    private readonly BatchExportWorker<GroupExporter> _backgroundExporter = new(exporter, factory, options);

    private bool _disposed;

    public bool IsActive => exporter.Count > 0;

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