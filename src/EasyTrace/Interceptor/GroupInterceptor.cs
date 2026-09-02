using EasyTrace.Activity;

namespace EasyTrace.Interceptor;

public class GroupInterceptor(List<ITraceActivityInterceptor> processors) : ITraceActivityInterceptor
{
    public void Start(scoped in TraceActivityRef activityRef)
    {
        foreach (var processor in processors)
        {
            processor.Start(activityRef);
        }
    }

    public void Stop(scoped in TraceActivityRef activityRef)
    {
        foreach (var processor in processors)
        {
            processor.Stop(activityRef);
        }
    }
}