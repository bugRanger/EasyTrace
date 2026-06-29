using EasyTrace.Activity;

namespace EasyTrace.Export.Batch;

public sealed class GroupExporter(ITraceActivityExporter[] exporters) : ITraceActivityExporter
{
    public void Export(scoped in TraceActivityRef activityRef)
    {
        foreach (var exporter in exporters)
        {
            exporter.Export(activityRef);
        }
    }
}