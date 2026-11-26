using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Activity.Context;

public class ContextThreadLocal(IFactory<TraceActivity> factory) : ITraceActivityContext
{
    private readonly ThreadLocal<TraceActivityContext> _contextThreadLocal = new(() => new TraceActivityContext(factory));

    public TraceActivity? Current
    {
        get => _contextThreadLocal.Value!.Current;
        set => _contextThreadLocal.Value!.Current = value;
    }

    public ITraceActivityPool Pool => _contextThreadLocal.Value!.Pool;
}