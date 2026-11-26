using EasyTrace.Export.Batch.Buffer;
using Microsoft.Extensions.ObjectPool;

namespace EasyTrace.Activity.Pool;

public class DefaultObjectPool(IFactory<TraceActivity> factory) : ITraceActivityPool
{
    private readonly ObjectPool<TraceActivity> _pool = new DefaultObjectPool<TraceActivity>(
        new PolicyWrapper(factory),
        maximumRetained: 64);

    public TraceActivity Rent() => _pool.Get();

    public void Return(TraceActivity item) => _pool.Return(item);

    private class PolicyWrapper(IFactory<TraceActivity> factory) : IPooledObjectPolicy<TraceActivity>
    {
        public TraceActivity Create() => factory.Create();

        public bool Return(TraceActivity activity) => true;
    }
}