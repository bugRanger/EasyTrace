using EasyTrace.Activity.Pool;
using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Activity.Context;

public class ContextAsyncLocal(IFactory<TraceActivity> factory) : ITraceActivityContext
{
    private readonly AsyncLocal<TraceActivity?> _currentLocal = new();


    public TraceActivity? Current
    {
        get => _currentLocal.Value;
        set => _currentLocal.Value = value;
    }

    public ITraceActivityPool Pool => new DefaultObjectPool(factory);
}