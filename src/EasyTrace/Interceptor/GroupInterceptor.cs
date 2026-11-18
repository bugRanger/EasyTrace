using EasyTrace.Activity;

namespace EasyTrace.Interceptor;

public class GroupInterceptor(ITraceActivityInterceptor[] processors)  : ITraceActivityInterceptor
{
    void ITraceActivityInterceptor.Start(scoped in TraceActivityRef activityRef)
    {
        foreach (var processor in processors)
        {
            processor.Start(activityRef);
        }
    }

    void ITraceActivityInterceptor.Stop(scoped in TraceActivityRef activityRef)
    {
        foreach (var processor in processors)
        {
            processor.Stop(activityRef);
        }
    }
}