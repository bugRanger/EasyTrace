using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Activity;

public class TraceActivityFactory(TraceActivityLimits limits) : IFactory<TraceActivity>
{
    public TraceActivity Create()
    {
        return new TraceActivity(limits);
    }
}