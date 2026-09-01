using EasyTrace.Activity;

namespace EasyTrace.Interceptor;

public interface ITraceActivityInterceptor
{
    void Start(scoped in TraceActivityRef activityRef);
    void Stop(scoped in TraceActivityRef activityRef);
}