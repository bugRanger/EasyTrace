using EasyTrace.Activity;

namespace EasyTrace.Export;

public interface ITraceActivityExporter
{
    void Export(in TraceActivityRef activityRef);
}