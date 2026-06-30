using EasyTrace.Activity;

namespace EasyTrace.Export;

public interface ITraceActivityExporter
{
    void Export(scoped in TraceActivityRef activityRef);
}
