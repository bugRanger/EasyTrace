using EasyTrace.Export.Batch.Buffer;

namespace EasyTrace.Activity.Context;

public class ContextAsyncLocal(IFactory<TraceActivity> factory) : ITraceActivityContext
{
    private readonly AsyncLocal<TraceActivityContext> _contextAsyncLocal = new();

    private TraceActivityContext Context => _contextAsyncLocal.Value ??= new TraceActivityContext(factory);

    public TraceActivity? Current
    {
        get => Context.Current;
        set => Context.Current = value;
    }

    public ITraceActivityPool Pool => Context.Pool;
}