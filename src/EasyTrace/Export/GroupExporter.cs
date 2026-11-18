using EasyTrace.Activity;

namespace EasyTrace.Export;

public sealed class GroupExporter(ITraceActivityExporter[] exporters) : ITraceActivityExporter
{
    public int Count => exporters.Length;

    void ITraceActivityExporter.Export(scoped in TraceActivityRef activityRef)
    {
        foreach (var exporter in exporters)
        {
            exporter.Export(activityRef);
        }
    }

    void ITraceActivityExporter.Flush()
    {
        foreach (var exporter in exporters)
        {
            exporter.Flush();
        }
    }
}