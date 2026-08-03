using EasyTrace.Activity;

namespace EasyTrace.Export.Jaeger;

public class JaegerExporter : ITraceActivityExporter
{
    public void Export(scoped in TraceActivityRef activityRef)
    {
        throw new NotImplementedException();
    }

    public void Flush()
    {
        throw new NotImplementedException();
    }
}