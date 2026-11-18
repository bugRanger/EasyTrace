using System.Collections.Concurrent;
using EasyTrace.Activity;

namespace EasyTrace.Export;

public sealed class GroupExporter : ITraceActivityExporter
{
    private readonly ConcurrentBag<ITraceActivityExporter> _exporters = [];

    public void Add(ITraceActivityExporter exporter) => _exporters.Add(exporter);

    public int Count => _exporters.Count;

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
}