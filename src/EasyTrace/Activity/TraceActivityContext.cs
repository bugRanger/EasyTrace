using EasyTrace.Activity.Pool;
using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Activity;

public class TraceActivityContext(IFactory<TraceActivity> factory) : ITraceActivityContext
{
    public TraceActivity? Current { get; set; }

    public ITraceActivityPool Pool { get; } = new UnboundedQueue(256, factory);
}