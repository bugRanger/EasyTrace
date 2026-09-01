using EasyTrace.Activity;
using EasyTrace.Export.Batch;

namespace EasyTrace.Export;

public sealed class GroupExporter : ITraceActivityExporter, IDisposable
{
    private readonly ITraceActivityExporter[] _exporters;
    private BatchExporter<GroupExporter>? _batchExporter;
    private bool _disposed;

    public GroupExporter(ITraceActivityExporter[] exporters, BatchExportOptions? batchExportOptions = null)
    {
        _exporters = exporters;

        if (batchExportOptions is not null)
        {
            _batchExporter = new BatchExporter<GroupExporter>(this, batchExportOptions);
        }
    }

    public void Handle(scoped in TraceActivityRef activityRef)
    {
        if (_batchExporter is not null)
        {
            _batchExporter.Append(in activityRef);
            return;
        }

        ((ITraceActivityExporter)this).Export(in activityRef);
        ((ITraceActivityExporter)this).Flush();
    }

    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        foreach (var exporter in _exporters)
        {
            exporter.Export(activityRef);
        }
    }

    void ITraceActivityExporter.Flush()
    {
        foreach (var exporter in _exporters)
        {
            exporter.Flush();
        }
    }

    ~GroupExporter()
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
            _batchExporter?.Dispose();
            _batchExporter = null;
        }

        _disposed = true;
    }
}