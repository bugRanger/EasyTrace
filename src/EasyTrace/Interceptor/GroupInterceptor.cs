using EasyTrace.Activity;

namespace EasyTrace.Interceptor;

public class GroupInterceptor(ITraceActivityInterceptor[] processors)  : ITraceActivityInterceptor
{
    public bool IsActive => processors.Length > 0;
    
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