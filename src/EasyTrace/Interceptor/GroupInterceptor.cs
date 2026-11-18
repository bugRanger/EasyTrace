using System.Collections.Concurrent;
using EasyTrace.Activity;

namespace EasyTrace.Interceptor;

public class GroupInterceptor : ITraceActivityInterceptor
{
    private readonly ConcurrentBag<ITraceActivityInterceptor> _processors = [];

    public void Add(ITraceActivityInterceptor exporter)
    {
        _processors.Add(exporter);
    }

    void ITraceActivityInterceptor.Start(scoped in TraceActivityRef activityRef)
    {
        foreach (var processor in _processors)
        {
            processor.Start(activityRef);
        }
    }

    void ITraceActivityInterceptor.Stop(scoped in TraceActivityRef activityRef)
    {
        foreach (var processor in _processors)
        {
            processor.Stop(activityRef);
        }
    }
}